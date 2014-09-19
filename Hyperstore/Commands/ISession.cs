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
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for session.
    /// </summary>
    /// <seealso cref="T:ISessionInformation"/>
    /// <seealso cref="T:IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface ISession : ISessionInformation, IDisposable
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the top level session.
        /// </summary>
        /// <value>
        ///  The top level session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ISession TopLevelSession { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Processes the command.
        /// </summary>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISessionResult Execute(params Hyperstore.Modeling.Commands.IDomainCommand[] command);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this session is disposing.
        /// </summary>
        /// <value>
        ///  <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsDisposing { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the locks.
        /// </summary>
        /// <value>
        ///  The locks.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ICollection<ILockInfo> Locks { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the isolation level.
        /// </summary>
        /// <value>
        ///  The isolation level.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        SessionIsolationLevel SessionIsolationLevel { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enlists the specified transaction.
        /// </summary>
        /// <param name="transaction">
        ///  The transaction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Enlist(ITransaction transaction);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Acquires a lock for a property.
        /// </summary>
        /// <param name="mode">
        ///  The lock mode.
        /// </param>
        /// <param name="id">
        ///  The owner id.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) Name of the property.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDisposable AcquireLock(LockType mode, Identity id, string propertyName = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Acquires a lock.
        /// </summary>
        /// <param name="mode">
        ///  The lock mode.
        /// </param>
        /// <param name="key">
        ///  The resource key to lock. The key object must provide a ToString() implementation to provide
        ///  the value key.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDisposable AcquireLock(LockType mode, object key);

        /// <summary>
        ///     Occurs when a session [completed].
        /// </summary>
        event EventHandler<SessionCompletingEventArgs> Completing;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Accepts the changes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void AcceptChanges();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an event to the session events list.
        /// </summary>
        /// <param name="event">
        ///  The event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddEvent(IEvent @event);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set the serialization mode.
        /// </summary>
        /// <param name="mode">
        ///  The lock mode.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void SetMode(SessionMode mode);
    }

    interface ISupportsCalculatedPropertiesTracking
    {
        IDisposable PushCalculatedPropertyTracker(CalculatedProperty tracker);

        CalculatedProperty CurrentTracker { get; }
    }
}