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
        public SchemaBuilder(IDomainManager store, ISchemaDefinition definition)
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
        public SchemaBuilder Set(string key, object value)
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
        public SchemaBuilder Using<TService>(Func<IDependencyResolver, TService> factory) where TService : class
        {
            _definition.Using<TService>(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Useses the id generator.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public SchemaBuilder UsingIdGenerator(Func<IDependencyResolver, Hyperstore.Modeling.HyperGraph.IIdGenerator> factory)
        {
            Using(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Subscribe in event bus.
        /// </summary>
        /// <param name="outputProperty">
        ///  The output property.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input property.
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
        ///  Creates the asynchronous.
        /// </summary>
        /// <returns>
        ///  The new asynchronous.
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
        public DomainBuilder(IDomainManager store, IDomainConfiguration definition)
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
        public DomainBuilder Using<TService>(Func<IDependencyResolver, TService> factory) where TService : class
        {
            _definition.Using<TService>(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Useses the id generator.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  A schema builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DomainBuilder UsingIdGenerator(Func<IDependencyResolver, Hyperstore.Modeling.HyperGraph.IIdGenerator> factory)
        {
            Using(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the asynchronous.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The new asynchronous.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public System.Threading.Tasks.Task<IDomainModel> CreateAsync(string name)
        {
            return _store.CreateDomainModelAsync(name, _definition);
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
        public DomainBuilder UsingModelElementFactory(Func<IDependencyResolver, IModelElementFactory> factory)
        {
            Using(factory);
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
        public DomainBuilder UsingCommandManager(Func<IDependencyResolver, ICommandManager> factory)
        {
            Using(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Subscribe in event bus.
        /// </summary>
        /// <param name="outputProperty">
        ///  The output property.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input property.
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
        ///  Generic type parameter.
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