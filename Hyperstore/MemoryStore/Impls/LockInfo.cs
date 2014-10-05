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
using System.Threading;

#endregion

namespace Hyperstore.Modeling
{
    internal class LockInfo : ILockInfo
    {
        private int _refCount;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the session.
        /// </summary>
        /// <value>
        ///  The identifier of the session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int SessionId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the lock.
        /// </summary>
        /// <value>
        ///  The lock.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ReaderWriterLockSlim Lock { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets a value indicating whether the promoted.
        /// </summary>
        /// <value>
        ///  true if promoted, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool Promoted { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the session status.
        /// </summary>
        /// <value>
        ///  The session status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public TransactionStatus SessionStatus { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the ressource.
        /// </summary>
        /// <value>
        ///  The ressource.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Ressource { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the mode.
        /// </summary>
        /// <value>
        ///  The mode.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public LockType Mode { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds reference.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void AddRef()
        {
            _refCount++;
            //Interlocked.Increment(ref _refCount);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Decrement reference.
        /// </summary>
        /// <returns>
        ///  An int.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int DecRef()
        {
            //return Interlocked.Decrement(ref _refCount);
            return --_refCount;
        }
    }
}