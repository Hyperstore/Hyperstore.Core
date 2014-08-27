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
    ///  Interface for schema configuration.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface ISchemaConfiguration
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
        ///  Useses the id generator.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaDefinition UsingIdGenerator(Func<IDependencyResolver, IIdGenerator> factory);

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
        ISchemaDefinition SubscribeToEventBus(Modeling.Messaging.ChannelPolicy outputProperty, Hyperstore.Modeling.Messaging.ChannelPolicy inputProperty = null);

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
        ISchemaDefinition Using<TService>(Func<IDependencyResolver, TService> factory) where TService : class;
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
        ///  Useses the id generator.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDomainConfiguration UsingIdGenerator(Func<IDependencyResolver, IIdGenerator> factory);

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
        IDomainConfiguration SubscribeToEventBus(Modeling.Messaging.ChannelPolicy outputProperty, Hyperstore.Modeling.Messaging.ChannelPolicy inputProperty = null);

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
        IDomainConfiguration Using<TService>(Func<IDependencyResolver, TService> factory) where TService : class;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Useses the model element resolver.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDomainConfiguration UsingModelElementFactory(Func<IDependencyResolver, IModelElementFactory> factory);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Useses the command manager.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDomainConfiguration UsingCommandManager(Func<IDependencyResolver, ICommandManager> factory);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Preload elements with.
        /// </summary>
        /// <param name="preloadAction">
        ///  The preload action.
        /// </param>
        /// <returns>
        ///  An IDomainConfiguration.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDomainConfiguration PreloadElementsWith(Action<IDomainModel> preloadAction);
    }
}
