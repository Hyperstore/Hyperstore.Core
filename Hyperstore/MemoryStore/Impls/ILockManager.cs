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
using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for lock manager.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface ILockManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Acquires the lock.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="ressource">
        ///  .
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDisposable AcquireLock(ISession session, object ressource, LockType mode);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the locks.
        /// </summary>
        /// <param name="locks">
        ///  The locks.
        /// </param>
        /// <param name="sessionAborted">
        ///  if set to <c>true</c> [session aborted].
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ReleaseLocks(ICollection<ILockInfo> locks, bool sessionAborted);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the specified ressource has lock.
        /// </summary>
        /// <param name="ressource">
        ///  The ressource.
        /// </param>
        /// <returns>
        ///  A LockType.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        LockType HasLock(object ressource);
    }
}