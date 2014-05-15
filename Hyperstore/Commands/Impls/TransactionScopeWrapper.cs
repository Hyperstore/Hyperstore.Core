// Copyright 2014 Zenasoft.  All rights reserved.
//
// This file is part of Hyperstore.
//
//    Hyperstore is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Hyperstore is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Hyperstore.  If not, see <http://www.gnu.org/licenses/>.

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