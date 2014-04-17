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

#endregion

namespace Hyperstore.Modeling.Events
{
    internal class PropagateEventDispatcher : IEventDispatcher
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the event.
        /// </summary>
        /// <param name="event">
        ///  The event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void HandleEvent(IEvent @event)
        {
            Contract.Requires(@event, "@event");

            Session.Current.AddEvent(@event);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the specified handler.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="handler">
        ///  The handler.
        /// </param>
        /// <param name="domainModel">
        ///  (Optional) the domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Register(IEventHandler handler, string domainModel = null)
        {
            throw new NotImplementedException();
        }
    }
}