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