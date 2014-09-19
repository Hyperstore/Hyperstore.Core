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