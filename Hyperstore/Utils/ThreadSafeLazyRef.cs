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
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// from https://github.com/aspnet/EntityFramework/blob/dev/src/EntityFramework/Utilities/ThreadSafeLazyRef.cs
namespace Hyperstore.Modeling.Utils
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A thread safe lazy reference.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerStepThrough]
    public sealed class ThreadSafeLazyRef<T>
    where T : class
    {
        private Func<T> _initializer;
        private object _syncLock;
        private T _value;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="initializer">
        ///  The initializer.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ThreadSafeLazyRef(Func<T> initializer)
        {
            Contract.Requires(initializer, "initializer");
            _initializer = initializer;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    var syncLock = new object();
                    syncLock
                    = Interlocked.CompareExchange(ref _syncLock, syncLock, null)
                    ?? syncLock;
                    lock (syncLock)
                    {
                        if (_value == null)
                        {
                            _value = _initializer();
                            _syncLock = null;
                            _initializer = null;
                        }
                    }
                }
                return _value;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Exchange value.
        /// </summary>
        /// <param name="newValueCreator">
        ///  The new value creator.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void ExchangeValue(Func<T, T> newValueCreator)
        {
            Contract.Requires(newValueCreator, "newValueCreator");
            T originalValue, newValue;
            do
            {
                originalValue = Value;
                newValue = newValueCreator(originalValue);
                if (ReferenceEquals(newValue, originalValue))
                {
                    return;
                }
            }
            while (Interlocked.CompareExchange(ref _value, newValue, originalValue) != originalValue);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value>
        ///  true if this instance has value, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool HasValue
        {
            get { return _value != null; }
        }
    }
}
