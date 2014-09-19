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
 
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Bitfield of flags for specifying DomainBehavior.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum DomainBehavior
    {
        /// <summary>
        ///  Standard
        /// </summary>
        Standard,
        /// <summary>
        ///  Disable L1 cache
        /// </summary>
        DisableL1Cache = 1,
        /// <summary>
        ///  Element form this schema are observables (Raises Notification when a property or a collection change)
        /// </summary>
        Observable = 2
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for domain configuration.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IDomainConfiguration
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare the scoped services container.
        /// </summary>
        /// <param name="container">
        ///  The default services container.
        /// </param>
        /// <returns>
        ///  An services container.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IServicesContainer PrepareScopedContainer(IServicesContainer container);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the in event bus.
        /// </summary>
        /// <param name="outputProperty">
        ///  The output property.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input property.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        void SubscribeToEventBus(Modeling.Messaging.ChannelPolicy outputProperty, Hyperstore.Modeling.Messaging.ChannelPolicy inputProperty = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Useses the specified factory.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        void Using<TService>(Func<IServicesContainer, TService> factory) where TService : class;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a property.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void SetProperty(string key, object value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the preload action.
        /// </summary>
        /// <param name="preloadAction">
        ///  The preload action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ExecutePreloadAction(Action<IDomainModel> preloadAction);
    }
}
