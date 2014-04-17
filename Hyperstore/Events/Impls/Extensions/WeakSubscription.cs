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

#endregion

namespace Hyperstore.Modeling.Events
{
    /// <summary>
    ///     Weak subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class WeakSubscription<T> : IDisposable, IObserver<T>
    {
        private readonly WeakReference _reference;
        private readonly IDisposable _subscription;
        private bool _disposed;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="observable">
        ///  The observable.
        /// </param>
        /// <param name="observer">
        ///  The observer.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public WeakSubscription(IObservable<T> observable, IObserver<T> observer)
        {
            _reference = new WeakReference(observer);
            _subscription = observable.Subscribe(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _subscription.Dispose();
            }
        }

        void IObserver<T>.OnCompleted()
        {
            var observer = (IObserver<T>) _reference.Target;
            if (observer != null)
                observer.OnCompleted();
            else
                Dispose();
        }

        void IObserver<T>.OnError(Exception error)
        {
            var observer = (IObserver<T>) _reference.Target;
            if (observer != null)
                observer.OnError(error);
            else
                Dispose();
        }

        void IObserver<T>.OnNext(T value)
        {
            var observer = (IObserver<T>) _reference.Target;
            if (observer != null)
                observer.OnNext(value);
            else
                Dispose();
        }
    }
}