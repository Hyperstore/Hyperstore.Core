//-------------------------------------------------------------------------------------------------
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
//-------------------------------------------------------------------------------------------------
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
        ///  Prepare dependency resolver.
        /// </summary>
        /// <param name="defaultDependencyResolver">
        ///  The default dependency resolver.
        /// </param>
        /// <returns>
        ///  An IDependencyResolver.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDependencyResolver PrepareDependencyResolver(IDependencyResolver defaultDependencyResolver);

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
        void Using<TService>(Func<IDependencyResolver, TService> factory) where TService : class;

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
