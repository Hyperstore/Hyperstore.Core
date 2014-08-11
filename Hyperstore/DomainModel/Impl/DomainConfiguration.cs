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
    ///  A domain configuration.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IDomainConfiguration"/>
    ///-------------------------------------------------------------------------------------------------
    public class DomainConfiguration : IDomainConfiguration
    {
        private readonly List<Action<IDependencyResolver>> _factories = new List<Action<IDependencyResolver>>();
        internal const string PreloadActionKey = "{A8D153D6-EEED-4841-9106-A508CAAB2BCF}";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Uses the memory eviction policy.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainConfiguration UsesMemoryEvictionPolicy(Func<IDependencyResolver, IEvictionPolicy> factory)
        {
            Uses(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Uses the id generator.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainConfiguration UsesIdGenerator(Func<IDependencyResolver, IIdGenerator> factory)
        {
            Uses(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Uses the model element resolver.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainConfiguration UsesModelElementFactory(Func<IDependencyResolver, IModelElementFactory> factory)
        {
            Uses(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Uses the command manager.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainConfiguration UsesCommandManager(Func<IDependencyResolver, ICommandManager> factory)
        {
            Uses(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Uses the specified factory.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainConfiguration Uses<TService>(Func<IDependencyResolver, TService> factory) where TService : class
        {
            Contract.Requires(factory, "factory");

            var f = new Action<IDependencyResolver>(r => r.Register<TService>(factory));
            _factories.Add(f);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare dependency resolver.
        /// </summary>
        /// <param name="parentResolver">
        ///  The default dependency resolver.
        /// </param>
        /// <returns>
        ///  An IDependencyResolver.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDependencyResolver IDomainConfiguration.PrepareDependencyResolver(IDependencyResolver parentResolver)
        {
            return PrepareDependencyResolver(parentResolver);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare dependency resolver.
        /// </summary>
        /// <param name="parentResolver">
        ///  The default dependency resolver.
        /// </param>
        /// <returns>
        ///  An IDependencyResolver.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IDependencyResolver PrepareDependencyResolver(IDependencyResolver parentResolver)
        {
            Contract.Requires(parentResolver, "parentResolver");

            foreach (var action in _factories)
            {
                action(parentResolver);
            }

            return parentResolver;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Uses.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="service">
        ///  The service.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainConfiguration Uses<TService>(TService service) where TService : class
        {
            Contract.Requires(service, "service");

            var f = new Action<IDependencyResolver>(r => r.Register(service));
            _factories.Add(f);
            return this;
        }

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
        public IDomainConfiguration RegisterInEventBus(Messaging.ChannelPolicy outputProperty, Messaging.ChannelPolicy inputProperty = null)
        {
            _factories.Add(new Action<IDependencyResolver>(r => r.Register(new Hyperstore.Modeling.RegistrationEventBusSetting { OutputProperty = outputProperty, InputProperty = inputProperty })));
            return this;
        }

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
        public IDomainConfiguration PreloadElementsWith(Action<IDomainModel> preloadAction)
        {
            Contract.Requires(preloadAction, "preloadAction");
            var f = new Action<IDependencyResolver>(r => r.RegisterSetting(PreloadActionKey, preloadAction));
            _factories.Add(f);
            return this;
        }
    }
}
