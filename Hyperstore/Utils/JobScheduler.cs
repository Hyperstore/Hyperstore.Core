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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling.Utils
{
    internal class JobScheduler : IDisposable
    {
        private Action _action;
        private readonly int _interval;
        private CancellationTokenSource _cancellationToken;
        private int _ready;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        /// <param name="interval">
        ///  The interval.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public JobScheduler(Action action, TimeSpan interval)
        {
            DebugContract.Requires(action);
            DebugContract.Requires(interval > TimeSpan.Zero);

            _action = action;
            _interval = (int)interval.TotalMilliseconds;
            _cancellationToken = new CancellationTokenSource();
#if !DEBUG
            Task.Factory.StartNew<bool>(Run, _cancellationToken.Token, TaskCreationOptions.LongRunning);
#endif
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();
            _cancellationToken = null;
            _action = null;
        }

        private bool Run(object arg)
        {
            try
            {
                while (true)
                {
                    if (_cancellationToken == null || _cancellationToken.IsCancellationRequested)
                        break;

                    if (_ready != 0)
                    {
                        var tmp = _action;
                        if (tmp == null)
                            break;
                        tmp();
                        Interlocked.Exchange(ref _ready, 0);
                    }

                    if (_cancellationToken == null || _cancellationToken.IsCancellationRequested)
                        break; 
                    
                    var t = Task.Delay(_interval, _cancellationToken.Token);
                    t.Wait();
                    if (t.IsCanceled)
                        break;
                }
            }
            catch
            {
            }
            return true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Request job.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void RequestJob()
        {
            Interlocked.Exchange(ref _ready, 1);
        }
    }
}