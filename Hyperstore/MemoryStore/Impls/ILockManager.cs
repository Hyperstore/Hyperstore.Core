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