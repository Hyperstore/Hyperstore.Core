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

using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Scopes;
using Hyperstore.Modeling.Events;
using System;
using System.Linq;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema builder.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class SchemaBuilder
    {
        private ISchemaDefinition _definition;
        private readonly IDomainManager _store;

        internal SchemaBuilder(IDomainManager store, ISchemaDefinition definition)
        {
            _store = store;
            _definition = definition;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set or override a configuration property
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
        public SchemaBuilder Set(string key, object value)
        {
            _definition.SetProperty(key, value);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register an action to execute before the schema will be activated
        /// </summary>
        /// <param name="action">
        ///  A lambda expression receiving the schema as paramater
        /// </param>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder PreloadAction(Action<IDomainModel> action)
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
        public SchemaBuilder Using<TService>(Func<IServicesContainer, TService> factory) where TService : class
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
        public SchemaBuilder AsObservable()
        {
            _definition.Behavior |= DomainBehavior.Observable;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Disable the L1 cache.
        /// </summary>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder DisableL1Cache()
        {
            _definition.Behavior |= DomainBehavior.DisableL1Cache;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Replace the current id generator.
        /// </summary>
        /// <param name="factory">
        ///  A id generator factory
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder UsingIdGenerator(Func<IServicesContainer, Hyperstore.Modeling.HyperGraph.IIdGenerator> factory)
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
        public SchemaBuilder SubscribeToEventBus(Messaging.ChannelPolicy outputProperty, Messaging.ChannelPolicy inputProperty = null)
        {
            _definition.SubscribeToEventBus(
                        outputProperty,
                        inputProperty
                    );
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new schema
        /// </summary>
        /// <returns>
        ///  The schema instance
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public System.Threading.Tasks.Task<ISchema> CreateAsync()
        {
            return _store.LoadSchemaAsync(_definition);
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
        ///  Set or override a configuration property
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
        ///  Provide a factory to create the new domain instance (If your domain doesn't have the standard constructor)
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
        ///  Replace the current id generator
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
        ///  Create a new domain model
        /// </summary>
        /// <param name="name">
        ///  The domain name.
        /// </param>
        /// <returns>
        ///  A new domain model
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public System.Threading.Tasks.Task<IDomainModel> CreateAsync(string name)
        {
            return _store.CreateDomainModelAsync(name, _definition, null, _factory);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new domain model of the specified type. The domain model must have a constructor with the following signature ctor(IServicesContainer services, string domainName).
        /// </summary>
        /// <typeparam name="TDomainModel">
        ///  Type of the domain model to create
        /// </typeparam>
        /// <param name="name">
        ///  The domain name.
        /// </param>
        /// <returns>
        ///  A new domain model
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public async System.Threading.Tasks.Task<TDomainModel> CreateAsync<TDomainModel>(string name) where TDomainModel : IDomainModel
        {
            var ctor = Hyperstore.Modeling.Utils.ReflectionHelper.GetConstructor(typeof(TDomainModel), new[] { typeof(IServicesContainer), typeof(string) }).FirstOrDefault();
            if (ctor == null)
                throw new Exception("{0} must provided a constructor with two parameters ctor(IServicesContainer services, string domainName) or use a domain factory");

            _factory = (s, n) => (IDomainModel)ctor.Invoke(new object[] { s, n });
            var domain = await _store.CreateDomainModelAsync(name, _definition, null, _factory);
            return (TDomainModel)domain;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Replace the current model element factory.
        /// </summary>
        /// <param name="factory">
        ///  A model element factory factory
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
        ///  Prepare a new schema builder using a definition
        /// </summary>
        /// <typeparam name="T">
        ///  Type of the schema definition to use
        /// </typeparam>
        /// <param name="schemas">
        ///  The schemas to act on.
        /// </param>
        /// <returns>
        ///  A SchemaBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static SchemaBuilder New<T>(this IModelList<ISchema> schemas) where T : ISchemaDefinition, new()
        {
            var store = schemas.Store as IDomainManager;
            return new SchemaBuilder(store, new T());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare a new schema builder from an existing definition
        /// </summary>
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
        public static SchemaBuilder New(this IModelList<ISchema> schemas, ISchemaDefinition definition)
        {
            Contract.Requires(definition, "definition");
            var store = schemas.Store as IDomainManager;
            return new SchemaBuilder(store, definition);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unload a schema
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
        ///  Prepare a new domain builder
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
        ///  Unload a domain or an extension
        /// </summary>
        /// <param name="models">
        ///  The models to act on.
        /// </param>
        /// <param name="scope">
        ///  domain or extension to unload
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