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