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

#endregion

namespace Hyperstore.Modeling
{
    internal class RelayObserver<T> : IObserver<T>, IDisposable
    {
        private readonly Action _onCompleted;
        private readonly Action _onError;
        private readonly Action<T> _onNext;

        internal RelayObserver(Action<T> onNext, Action onCompleted = null, Action onError = null)
        {
            Contract.Requires(onNext != null, "onNext");

            _onCompleted = onCompleted;
            _onError = onError;
            _onNext = onNext;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the completed action.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void OnCompleted()
        {
            if (_onCompleted != null)
                _onCompleted();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the error action.
        /// </summary>
        /// <param name="error">
        ///  The error.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void OnError(Exception error)
        {
            if (_onError != null)
                _onError();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the next action.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void OnNext(T value)
        {
            _onNext(value);
        }
    }
}