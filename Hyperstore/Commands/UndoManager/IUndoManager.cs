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

using Hyperstore.Modeling.Events;
using System;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for undo manager.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface IUndoManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Capacity of the stack.
        /// </summary>
        /// <value>
        ///  The capacity.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        int Capacity { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the save point.
        /// </summary>
        /// <value>
        ///  The save point.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Guid? SavePoint { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [can undo].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [can undo]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool CanUndo { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [can redo].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [can redo]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool CanRedo { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets a value indicating whether [enabled].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [enabled]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool Enabled { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the domain.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="dispatcher">
        ///  (Optional) the dispatcher.
        /// </param>
        /// <param name="eventFilter">
        ///  (Optional) a filter specifying the event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RegisterDomain(IDomainModel domainModel, IEventDispatcher dispatcher = null, Func<IUndoableEvent, bool> eventFilter = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Undoes this instance.
        /// </summary>
        /// <param name="toSavePoint">
        ///  (Optional)
        ///  Dequeue all the commands until the savePoint. Warning : If the savepoint is not in the stack,
        ///  all the commands will be dequeue.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Undo(Guid? toSavePoint = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Redoes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Redo();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the events dispatcher associated with a domain model.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <returns>
        ///  The event dispatcher.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEventDispatcher GetEventDispatcher(IDomainModel domainModel);

    }
}