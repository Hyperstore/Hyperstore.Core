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