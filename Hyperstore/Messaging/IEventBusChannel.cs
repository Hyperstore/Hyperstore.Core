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

using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for event bus channel.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventBusChannel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the filter described by filter.
        /// </summary>
        /// <param name="filter">
        ///  Specifies the filter.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RegisterFilter(ChannelFilter filter);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Starts the asynchronous.
        /// </summary>
        /// <param name="eventBus">
        ///  The event bus.
        /// </param>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task StartAsync(IEventBus eventBus);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sends the events.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void SendEvents(ISessionInformation session);
    }
}