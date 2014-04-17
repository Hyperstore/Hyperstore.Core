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
 
using System;
using System.Threading;

namespace Hyperstore.Modeling.Utils
{
    internal class Subject<T> : ISubjectWrapper<T>
    {
        private readonly object _gate = new object();
        private Exception _exception;
        private bool _isDisposed;
        private bool _isStopped;
        private ImmutableList<IObserver<T>> _observers;
        private readonly ISynchronizationContext _synchronizationContext;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public Subject(IDependencyResolver resolver)
        {
            this._observers = new ImmutableList<IObserver<T>>();
            _synchronizationContext = resolver.Resolve<ISynchronizationContext>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Notifies all subscribed observers about the end of the sequence.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void OnCompleted()
        {
            var os = default(IObserver<T>[]);
            lock (this._gate)
            {
                CheckDisposed();

                if (!this._isStopped)
                {
                    os = this._observers.Data;
                    this._observers = new ImmutableList<IObserver<T>>();
                    this._isStopped = true;
                }
            }

            if (os != null)
            {
                foreach (var o in os)
                {
                    if (_synchronizationContext == null)
                        o.OnCompleted();
                    else
                        _synchronizationContext.Send(() => o.OnCompleted());
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Notifies all subscribed observers with the exception.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///  <paramref name="error" /> is null.
        /// </exception>
        /// <param name="error">
        ///  The exception to send to all subscribed observers.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            var os = default(IObserver<T>[]);
            lock (this._gate)
            {
                CheckDisposed();

                if (!this._isStopped)
                {
                    os = this._observers.Data;
                    this._observers = new ImmutableList<IObserver<T>>();
                    this._isStopped = true;
                    this._exception = error;
                }
            }

            if (os != null)
            {
                foreach (var o in os)
                {
                    if (_synchronizationContext == null)
                        o.OnError(error);
                    else
                        _synchronizationContext.Send(() => o.OnError(error));
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Notifies all subscribed observers with the value.
        /// </summary>
        /// <param name="value">
        ///  The value to send to all subscribed observers.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void OnNext(T value)
        {
            var os = default(IObserver<T>[]);
            lock (this._gate)
            {
                CheckDisposed();

                if (!this._isStopped)
                    os = this._observers.Data;
            }

            if (os != null)
            {
                foreach (var o in os)
                {
                    if (_synchronizationContext == null)
                        o.OnNext(value);
                    else
                        _synchronizationContext.Send(() => o.OnNext(value));
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Subscribes an observer to the subject.
        /// </summary>
        /// <remarks>
        ///  IDisposable object that can be used to unsubscribe the observer from the subject.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///  <paramref name="observer" /> is null.
        /// </exception>
        /// <param name="observer">
        ///  Observer to subscribe to the subject.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            lock (this._gate)
            {
                CheckDisposed();

                if (!this._isStopped)
                {
                    this._observers = this._observers.Add(observer);
                    return new Subscription(this, observer);
                }
                if (this._exception != null)
                {
                    if (_synchronizationContext == null)
                        observer.OnError(this._exception);
                    else
                        _synchronizationContext.Send(() => observer.OnError(this._exception));
                    
                    return Disposables.Empty;
                }

                if (_synchronizationContext == null)
                    observer.OnCompleted();
                else
                    _synchronizationContext.Send(() => observer.OnCompleted());
                return Disposables.Empty;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unsubscribe all observers and release resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            lock (this._gate)
            {
                this._isDisposed = true;
                this._observers = null;
            }
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            lock (this._gate)
            {
                if (this._observers != null)
                    this._observers = this._observers.Remove(observer);
            }
        }

        private void CheckDisposed()
        {
            if (this._isDisposed)
                throw new ObjectDisposedException(string.Empty);
        }

        internal class ImmutableList<TElem>
        {
            private readonly TElem[] _data;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Default constructor.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public ImmutableList()
            {
                this._data = new TElem[0];
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="data">
            ///  The data.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public ImmutableList(TElem[] data)
            {
                this._data = data;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the data.
            /// </summary>
            /// <value>
            ///  The data.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public TElem[] Data
            {
                get { return this._data; }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Adds value.
            /// </summary>
            /// <param name="value">
            ///  The value.
            /// </param>
            /// <returns>
            ///  A list of.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public ImmutableList<TElem> Add(TElem value)
            {
                var newData = new TElem[this._data.Length + 1];
                Array.Copy(this._data, newData, this._data.Length);
                newData[this._data.Length] = value;
                return new ImmutableList<TElem>(newData);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Removes the given value.
            /// </summary>
            /// <param name="value">
            ///  The value.
            /// </param>
            /// <returns>
            ///  A list of.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public ImmutableList<TElem> Remove(TElem value)
            {
                var i = IndexOf(value);
                if (i < 0)
                    return this;
                var newData = new TElem[this._data.Length - 1];
                Array.Copy(this._data, 0, newData, 0, i);
                Array.Copy(this._data, i + 1, newData, i, this._data.Length - i - 1);
                return new ImmutableList<TElem>(newData);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Index of the given value.
            /// </summary>
            /// <param name="value">
            ///  The value.
            /// </param>
            /// <returns>
            ///  An int.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public int IndexOf(TElem value)
            {
                for (var i = 0; i < this._data.Length; ++i)
                {
                    if (this._data[i].Equals(value))
                        return i;
                }
                return -1;
            }
        }

        private class Subscription : IDisposable
        {
            private IObserver<T> _observer;
            private Subject<T> _subject;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="subject">
            ///  The subject.
            /// </param>
            /// <param name="observer">
            ///  The observer.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public Subscription(Subject<T> subject, IObserver<T> observer)
            {
                this._subject = subject;
                this._observer = observer;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
            ///  resources.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Dispose()
            {
                var o = Interlocked.Exchange(ref this._observer, null);
                if (o == null)
                    return;
                this._subject.Unsubscribe(o);
                this._subject = null;
            }
        }
    }
}