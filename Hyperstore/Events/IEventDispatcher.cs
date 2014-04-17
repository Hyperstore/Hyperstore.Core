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
 
namespace Hyperstore.Modeling.Events
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for event dispatcher.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventDispatcher
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the event.
        /// </summary>
        /// <param name="event">
        ///  The event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void HandleEvent(IEvent @event);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the specified handler.
        /// </summary>
        /// <param name="handler">
        ///  The handler.
        /// </param>
        /// <param name="domainModel">
        ///  (Optional) the domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Register(IEventHandler handler, string domainModel = null);
    }
}