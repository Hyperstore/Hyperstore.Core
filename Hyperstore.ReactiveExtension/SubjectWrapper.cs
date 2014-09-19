//	Copyright � 2013 - 2014, Alain Metge. All rights reserved.
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
using Hyperstore.Modeling;

#endregion

namespace Hyperstore.ReactiveExtension
{
    internal class Subject<T> : ISubjectWrapper<T>
    {
        private readonly System.Reactive.Subjects.Subject<T> _subject;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public Subject()
        {
            _subject = new System.Reactive.Subjects.Subject<T>();
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
            _subject.OnNext(value);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the completed action.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Subscribes the given observer.
        /// </summary>
        /// <param name="observer">
        ///  The observer.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subject.Subscribe(observer);
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
            _subject.OnError(error);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            _subject.Dispose();
        }
    }
}