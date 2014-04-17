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
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for event bus.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventBus
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in EventBusStarted events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        event EventHandler EventBusStarted;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the channels.
        /// </summary>
        /// <value>
        ///  The channels.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IEventBusChannel> Channels { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Starts the asynchronous.
        /// </summary>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task OpenAsync(params IEventBusChannel[] channels);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IHyperstore Store { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default event dispatcher.
        /// </summary>
        /// <value>
        ///  The default event dispatcher.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Events.IEventDispatcher DefaultEventDispatcher { get;  }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers this instance.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="outputProperty">
        ///  The output property.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RegisterDomainPolicies(IDomainModel domain, ChannelPolicy outputProperty, ChannelPolicy inputProperty=null);
    }
}