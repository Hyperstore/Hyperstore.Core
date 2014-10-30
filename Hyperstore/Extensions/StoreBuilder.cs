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
        private const string platformTypeName = "Hyperstore.Modeling.Platform.PlatformServicesInstance, Hyperstore.Platform";

        private StoreOptions _options = StoreOptions.None;
        private List<Assembly> _assemblies;
        private ServicesContainer _services = new ServicesContainer();
        private string _id;

        private StoreBuilder()
        {
            var platformfactoryType = Type.GetType(platformTypeName, false);
            if (platformfactoryType != null)
            {
                Activator.CreateInstance(platformfactoryType); // Initialize platfom services singleton and set its current property
            }
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

            if (_assemblies == null)
                _assemblies = new List<Assembly>();
            if (assemblies.Length > 0)
            {
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
        public StoreBuilder WithId(string id)
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
        public static async Task<IDomainModel> CreateDomain<T>(string name) where T : class, ISchemaDefinition, new()
        {
            Contract.RequiresNotEmpty(name, "name");

            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<T>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync(name);
            return domain;
        }
    }
}
