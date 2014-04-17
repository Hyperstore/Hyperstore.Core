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
        IExecutionResult Execute(params Hyperstore.Modeling.Commands.IDomainCommand[] command);

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
}