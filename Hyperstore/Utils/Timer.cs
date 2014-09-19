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
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A timer.
    /// </summary>
    /// <seealso cref="T:System.IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    public sealed class Timer : IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken;

        private Timer(Action action, int ms)
        {
            _cancellationToken = new CancellationTokenSource();

            Task.Run(() =>
            {
                Task.Delay(ms, _cancellationToken.Token)
                        .ContinueWith(t =>
                        {
                            if (!_cancellationToken.IsCancellationRequested)
                                action();
                        });
            }, _cancellationToken.Token);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a new IDisposable.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        /// <param name="timeout">
        ///  The timeout.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IDisposable Create(Action action, TimeSpan timeout)
        {
            Contract.Requires(action, "action");
            Contract.Requires(timeout.TotalMilliseconds > 0, "timeout");
            return new Timer(action, (int) timeout.TotalMilliseconds);
        }
    }
}