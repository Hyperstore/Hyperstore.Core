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