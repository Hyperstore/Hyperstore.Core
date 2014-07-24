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

#region Imports

using System;
using System.Collections.Generic;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema definition.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaDefinition"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class SchemaDefinition : ISchemaDefinition
    {
        private readonly List<Action<IDependencyResolver>> _factories = new List<Action<IDependencyResolver>>();
        private readonly string _name;
        private readonly DomainBehavior _behavior;

        protected DomainBehavior Behavior { get { return _behavior; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="behavior">
        ///  (Optional) the behavior.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaDefinition(string name, DomainBehavior behavior = DomainBehavior.Standard)
        {
            Contract.Requires(name, "name");
            Conventions.CheckValidDomainName(name);

            _name = name;
            _behavior = behavior;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name of the schema.
        /// </summary>
        /// <value>
        ///  The name of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string ISchemaDefinition.SchemaName { get { return _name; } }

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
        public ISchemaDefinition UsesIdGenerator(Func<IDependencyResolver, IIdGenerator> factory)
        {
            Uses(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Useses the query adapter.
        /// </summary>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <returns>
        ///  An ISchemaDefinition.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaDefinition UsesGraphAdapter(Func<IDependencyResolver, IGraphAdapter> factory)
        {
            Uses(factory);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Useses the specified factory.
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
        public ISchemaDefinition Uses<TService>(Func<IDependencyResolver, TService> factory) where TService : class
        {
            Contract.Requires(factory, "factory");

            var f = new Action<IDependencyResolver>(r => r.Register<TService>(factory));
            _factories.Add(f);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [schema loaded].
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ISchemaDefinition.OnSchemaLoaded(ISchema schema)
        {
            DebugContract.Requires(schema);
            OnSchemaLoaded(schema);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the schema loaded action.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void OnSchemaLoaded(ISchema schema)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Defines the schema.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ISchemaDefinition.DefineSchema(ISchema schema)
        {
            DebugContract.Requires(schema);
            DefineSchema(schema);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Defines the schema.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected abstract void DefineSchema(ISchema schema);

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
        IDependencyResolver ISchemaConfiguration.PrepareDependencyResolver(IDependencyResolver parentResolver)
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
        public ISchemaDefinition Uses<TService>(TService service) where TService : class
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
        public ISchemaDefinition RegisterInEventBus(Messaging.ChannelPolicy outputProperty, Messaging.ChannelPolicy inputProperty = null)
        {
            _factories.Add(new Action<IDependencyResolver>(r => r.Register(new Hyperstore.Modeling.RegistrationEventBusSetting { OutputProperty = outputProperty, InputProperty = inputProperty })));
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a schema.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <returns>
        ///  The new schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchema ISchemaDefinition.CreateSchema(IDependencyResolver resolver)
        {
            DebugContract.Requires(resolver);
            return CreateSchema(resolver);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a schema.
        /// </summary>
        /// <param name="domainResolver">
        ///  The domain resolver.
        /// </param>
        /// <returns>
        ///  The new schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ISchema CreateSchema(IDependencyResolver domainResolver)
        {
            return new Hyperstore.Modeling.Metadata.DomainSchema(_name, domainResolver, _behavior);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Loads dependent schemas. </summary>
        /// <param name="store">    The store. </param>
        ///-------------------------------------------------------------------------------------------------
        void ISchemaDefinition.LoadDependentSchemas(IHyperstore store)
        {
            DebugContract.Requires(store);
            LoadDependentSchemas(store);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads dependent schemas.
        /// </summary>
        /// <param name="store">
        ///  The store.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void LoadDependentSchemas(IHyperstore store)
        {
        }
    }
}