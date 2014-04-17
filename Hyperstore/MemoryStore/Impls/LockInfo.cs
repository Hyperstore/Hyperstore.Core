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
        public Guid SessionId { get; set; }

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