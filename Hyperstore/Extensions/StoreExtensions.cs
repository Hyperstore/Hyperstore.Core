﻿//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
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
using Hyperstore.Modeling.Scopes;
using Hyperstore.Modeling.Events;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema builder.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    public class SchemaBuilder<T> where T: class, ISchemaDefinition 
    {
        private T _definition;
        private readonly IDomainManager _store;

        internal SchemaBuilder(IDomainManager store, T definition)
        {
            _store = store;
            _definition = definition;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set or override a configuration property.
        /// </summary>
        /// <param name="key">
        ///  The property key.
        /// </param>
        /// <param name="value">
        ///  The property value.
        /// </param>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder<T> Set(string key, object value)
        {
            _definition.SetProperty(key, value);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register an action to execute before the schema will be activated.
        /// </summary>
        /// <param name="action">
        ///  A lambda expression receiving the schema as paramater.
        /// </param>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder<T> PreloadAction(Action<IDomainModel> action)
        {
            _definition.ExecutePreloadAction(action);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register a service.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <param name="factory">
        ///  The service factory.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder<T> Using<TService>(Func<IServicesContainer, TService> factory) where TService : class
        {
            _definition.Using<TService>(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Define the domain as observable (Raises INotifyPropertyChanged and ICollectionChanged)
        /// </summary>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder<T> AsObservable()
        {
            _definition.Behavior |= DomainBehavior.Observable;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Replace the current id generator.
        /// </summary>
        /// <param name="factory">
        ///  A id generator factory.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder<T> UsingIdGenerator(Func<IServicesContainer, Hyperstore.Modeling.HyperGraph.IIdGenerator> factory)
        {
            Using(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Subscribe to event bus notifications.
        /// </summary>
        /// <param name="outputProperty">
        ///  The output policy.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input policy.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder<T> SubscribeToEventBus(Messaging.ChannelPolicy outputProperty, Messaging.ChannelPolicy inputProperty = null)
        {
            _definition.SubscribeToEventBus(
                        outputProperty,
                        inputProperty
                    );
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new schema.
        /// </summary>
        /// <returns>
        ///  The schema instance.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public System.Threading.Tasks.Task<ISchema<T>> CreateAsync() 
        {
            return _store.LoadSchemaAsync<T>(_definition);
        }

    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A domain builder.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class DomainBuilder
    {
        private readonly IDomainManager _store;
        private readonly IDomainConfiguration _definition;
        private Func<IServicesContainer, string, IDomainModel> _factory = null;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="store">
        ///  The store.
        /// </param>
        /// <param name="definition">
        ///  The definition.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        internal DomainBuilder(IDomainManager store, IDomainConfiguration definition)
        {
            _store = store;
            _definition = definition;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set or override a configuration property.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder Set(string key, object value)
        {
            _definition.SetProperty(key, value);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Preload action.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        /// <returns>
        ///  A DomainBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder PreloadAction(Action<IDomainModel> action)
        {
            _definition.ExecutePreloadAction(action);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register a service.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <param name="factory">
        ///  The service factory.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder Using<TService>(Func<IServicesContainer, TService> factory) where TService : class
        {
            _definition.Using<TService>(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Provide a factory to create the new domain instance (If your domain doesn't have the standard
        ///  constructor)
        /// </summary>
        /// <param name="factory">
        ///  The domain factory.
        /// </param>
        /// <returns>
        ///  A DomainBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder UsingDomainFactory(Func<IServicesContainer, string, IDomainModel> factory)
        {
            _factory = factory;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Replace the current id generator.
        /// </summary>
        /// <param name="factory">
        ///  The id generator factory.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder UsingIdGenerator(Func<IServicesContainer, Hyperstore.Modeling.HyperGraph.IIdGenerator> factory)
        {
            Using(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new domain model.
        /// </summary>
        /// <param name="name">
        ///  The domain name.
        /// </param>
        /// <returns>
        ///  A new domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public System.Threading.Tasks.Task<IDomainModel> CreateAsync(string name)
        {
            return _store.CreateDomainModelAsync(name, _definition, null, _factory);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new domain model of the specified type. The domain model must have a constructor
        ///  with the following signature ctor(IServicesContainer services, string domainName).
        /// </summary>
        /// <exception cref="CriticalException">
        ///  Thrown when a Critical error condition occurs.
        /// </exception>
        /// <typeparam name="TDomainModel">
        ///  Type of the domain model to create.
        /// </typeparam>
        /// <param name="name">
        ///  The domain name.
        /// </param>
        /// <returns>
        ///  A new domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public async System.Threading.Tasks.Task<TDomainModel> CreateAsync<TDomainModel>(string name) where TDomainModel : IDomainModel
        {
            var ctor = Hyperstore.Modeling.Utils.ReflectionHelper.GetConstructor(typeof(TDomainModel), new[] { typeof(IServicesContainer), typeof(string) }).FirstOrDefault();
            if (ctor == null)
                throw new CriticalException("{0} must provided a constructor with two parameters ctor(IServicesContainer services, string domainName) or use a domain factory");

            _factory = (s, n) => (IDomainModel)ctor.Invoke(new object[] { s, n });
            var domain = await _store.CreateDomainModelAsync(name, _definition, null, _factory);
            return (TDomainModel)domain;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Replace the current model element factory.
        /// </summary>
        /// <param name="factory">
        ///  A model element factory factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder UsingModelElementFactory(Func<IServicesContainer, IModelElementFactory> factory)
        {
            Using(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Replace the current command manager.
        /// </summary>
        /// <param name="factory">
        ///  A command manager factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder UsingCommandManager(Func<IServicesContainer, ICommandManager> factory)
        {
            Using(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Subscribe to event bus notifications.
        /// </summary>
        /// <param name="outputProperty">
        ///  The output policy.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input policy.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder SubscribeToEventBus(Messaging.ChannelPolicy outputProperty, Messaging.ChannelPolicy inputProperty = null)
        {
            _definition.SubscribeToEventBus(outputProperty, inputProperty);
            return this;
        }

    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema extensions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class SchemaExtensions
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates all in this collection.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="list">
        ///  The list to act on.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IEnumerable<T> All<T>(this IModelList<T> list) where T:class, IDomainModel
        {
            var scopes = list as IScopeManager<T>;
            return scopes.GetScopes(ScopesSelector.Enabled, Session.Current != null ? Session.Current.SessionId : 0);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare a new schema builder using a definition.
        /// </summary>
        /// <typeparam name="T">
        ///  Type of the schema definition to use.
        /// </typeparam>
        /// <param name="schemas">
        ///  The schemas to act on.
        /// </param>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static SchemaBuilder<T> New<T>(this IModelList<ISchema> schemas) where T : class, ISchemaDefinition, new()
        {
            var store = schemas.Store as IDomainManager;
            return new SchemaBuilder<T>(store, new T());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare a new schema builder from an existing definition.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="schemas">
        ///  The schemas to act on.
        /// </param>
        /// <param name="definition">
        ///  The definition.
        /// </param>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static SchemaBuilder<T> New<T>(this IModelList<ISchema> schemas, T definition) where T : class, ISchemaDefinition
        {
            Contract.Requires(definition, "definition");
            var store = schemas.Store as IDomainManager;
            return new SchemaBuilder<T>(store, definition);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unload a schema.
        /// </summary>
        /// <param name="models">
        ///  The models to act on.
        /// </param>
        /// <param name="scope">
        ///  The scope.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public static void Unload(this IModelList<ISchema> models, ISchema scope)
        {
            Contract.Requires(scope, "scope");
            var store = models.Store as IDomainManager;
            store.UnloadSchemaOrExtension(scope);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A domain configuration extensions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class DomainConfigurationExtensions
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare a new domain builder.
        /// </summary>
        /// <param name="models">
        ///  The models to act on.
        /// </param>
        /// <param name="definition">
        ///  (Optional) the definition.
        /// </param>
        /// <returns>
        ///  A DomainBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static DomainBuilder New(this IModelList<IDomainModel> models, IDomainConfiguration definition = null)
        {
            var store = models.Store as IDomainManager;
            return new DomainBuilder(store, definition ?? new DomainConfiguration());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unload a domain or an extension.
        /// </summary>
        /// <param name="models">
        ///  The models to act on.
        /// </param>
        /// <param name="scope">
        ///  domain or extension to unload.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public static void Unload(this IModelList<IDomainModel> models, IDomainModel scope)
        {
            Contract.Requires(scope, "scope");
            var store = models.Store as IDomainManager;
            store.UnloadDomainOrExtension(scope);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A store extensions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class StoreExtensions
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  An IHyperstore extension method that creates a relationship.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="store">
        ///  The store to act on.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="id">
        ///  (Optional) the identifier.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static T CreateRelationship<T>(this IHyperstore store, IModelElement start, IModelElement end, Identity id = null) where T : IModelRelationship
        {
            Contract.Requires(store != null, "store");
            Contract.Requires(start != null, "start");
            Contract.Requires(end != null, "end");

            if (Session.Current == null)
                throw new SessionRequiredException();
            var domain = start.DomainModel;
            var schema = store.GetSchemaRelationship<T>();
            var cmd = new AddRelationshipCommand(schema, start, end, id);
            Session.Current.Execute(cmd);
            return (T)cmd.Relationship;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  An IHyperstore extension method that creates a relationship.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <param name="store">
        ///  The store to act on.
        /// </param>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="id">
        ///  (Optional) the identifier.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IModelRelationship CreateRelationship(this IHyperstore store, ISchemaRelationship schema, IModelElement start, IModelElement end, Identity id = null)
        {
            Contract.Requires(store != null, "store");
            Contract.Requires(schema != null, "schema");
            Contract.Requires(start != null, "start");
            Contract.Requires(end != null, "end");
            if (Session.Current == null)
                throw new SessionRequiredException();

            var domain = start.DomainModel;
            var cmd = new AddRelationshipCommand(schema, start, end, id);
            Session.Current.Execute(cmd);
            return cmd.Relationship;
        }
    }
}