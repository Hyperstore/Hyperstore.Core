//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
//
//		This file is part of Hyperstore (http://www.hyperstore.org)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
 
#region Imports

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Hyperstore.Modeling.Commands
{

    internal class HyperstoreTransactionScope : ITransactionScope
    {
        private SessionIsolationLevel _sessionIsolationLevel;
        private TimeSpan _timeout;
        private Session _session;
        private bool _completed;
        private IDisposable _timer;

        private List<ISessionEnlistmentNotification> Transactions
        {
            get
            {
                return _session.Enlistment;
            }
        }

        public HyperstoreTransactionScope(Session session, SessionIsolationLevel sessionIsolationLevel, TimeSpan timeSpan)
        {
            this._sessionIsolationLevel = sessionIsolationLevel;
            this._timeout = timeSpan;
            this._session = session;

            if (TimeSpan.Zero != timeSpan)
                _timer = Hyperstore.Modeling.Utils.Timer.Create(OnTimeOut, timeSpan);
        }

        private void OnTimeOut()
        {
            // TODO revoir le comportement
            Dispose();
            throw new TimeoutException();
        }

        public void Complete()
        {
            _completed = true;
        }

        public void Dispose()
        {
            if (_session == null)
                throw new ObjectDisposedException("Session");

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if (Transactions != null)
            {
                if (_completed)
                {
                    if (Transactions.All(t => t.NotifyPrepare()))
                    {
                        foreach (var t in Transactions)
                            t.NotifyCommit();
                        return;
                    }
                }

                foreach (var t in Transactions)
                    t.NotifyRollback();

                Transactions.Clear();
            }
            _session = null;
        }


        public void Enlist(ITransaction transaction)
        {
            Transactions.Add((ISessionEnlistmentNotification)transaction);
        }
    }

}