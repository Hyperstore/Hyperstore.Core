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