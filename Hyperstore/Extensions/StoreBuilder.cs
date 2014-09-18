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

using Hyperstore.Modeling.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Store builder is used to create a new store
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public sealed class StoreBuilder
    {
        private StoreOptions _options = StoreOptions.None;
        private List<Assembly> _assemblies;
        private IServicesContainer _services = new ServicesContainer();
        private Guid? _id;

        private StoreBuilder()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Intialize a new definition context
        /// </summary>
        /// <returns>
        ///  A StoreBuilder instance
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static StoreBuilder New()
        {
            return new StoreBuilder();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enable scoping for domain model
        /// </summary>
        /// <returns>
        ///  A StoreBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public StoreBuilder EnableScoping()
        {
            _options |= StoreOptions.EnableScopings;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Find and integrate types with composition attribute (CommandHandler, CommandInterceptor...).
        /// </summary>
        /// <param name="assemblies">
        ///  A list of assemblies containing the type to integrate.
        /// </param>
        /// <returns>
        ///  A StorBuilder
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public StoreBuilder ComposeWith(params Assembly[] assemblies)
        {
            if (assemblies.Length > 0)
            {
                if (_assemblies == null)
                    _assemblies = new List<Assembly>();
                _assemblies.AddRange(assemblies);
            }
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Force the store id
        /// </summary>
        /// <param name="id">
        ///  The new store identifier.
        /// </param>
        /// <returns>
        ///  A StoreBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public StoreBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register a service in the store context
        /// </summary>
        /// <typeparam name="T">
        ///  Type of the service
        /// </typeparam>
        /// <param name="service">
        ///  The factory used to instanciate the service
        /// </param>
        /// <param name="lifecycle">
        ///  The service life cycle
        /// </param>
        /// <returns>
        ///  A StoreBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public StoreBuilder Using<T>(Func<IServicesContainer, T> service, ServiceLifecycle lifecycle = ServiceLifecycle.Scoped) where T : class
        {
            _services.Register<T>(service, lifecycle);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the store.
        /// </summary>
        /// <returns>
        ///  The new store.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public async Task<IHyperstore> CreateAsync()
        {
            if (_assemblies != null)
                await _services.ComposeAsync(_assemblies.ToArray());

            var store = new Store(_services, _options, _id);
            return store;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new domain and load the specified schema
        /// </summary>
        /// <typeparam name="T">
        ///  The definition of the schema to load
        /// </typeparam>
        /// <param name="name">
        ///  Name of the new domain
        /// </param>
        /// <returns>
        ///  A new domain instance.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static async Task<IDomainModel> CreateDomain<T>(string name) where T : ISchemaDefinition, new()
        {
            Contract.RequiresNotEmpty(name, "name");

            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<T>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync(name);
            return domain;
        }
    }
}
