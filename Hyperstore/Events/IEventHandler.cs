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

using System.Collections.Generic;
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling.Events
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for event handler.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventHandler
    {
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for event handler.
    /// </summary>
    /// <typeparam name="TEvent">
    ///  Type of the in t event.
    /// </typeparam>
    /// <seealso cref="T:IEventHandler"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IEvent
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the specified domain model.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="event">
        ///  The event.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process handle in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IDomainCommand> Handle(IDomainModel domainModel, TEvent @event);
    }
}