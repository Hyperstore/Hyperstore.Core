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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling.Utils
{
    internal class JobScheduler : IDisposable
    {
        private readonly Action _action;
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
                        _action();
                        Interlocked.Exchange(ref _ready, 0);
                    }
                    
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