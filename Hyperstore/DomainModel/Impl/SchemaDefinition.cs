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
    public abstract class SchemaDefinition : DomainConfiguration, ISchemaDefinition
    {
        private readonly string _name;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the behavior.
        /// </summary>
        /// <value>
        ///  The behavior.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public DomainBehavior Behavior { get; set; }

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
            Behavior = behavior;
            if ((Behavior & DomainBehavior.Observable) == DomainBehavior.Observable)
            {
                Behavior &= ~DomainBehavior.DisableL1Cache;
            }
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
        ///  Creates a schema.
        /// </summary>
        /// <param name="services">
        ///  The services.
        /// </param>
        /// <returns>
        ///  The new schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchema ISchemaDefinition.CreateSchema(IServicesContainer services)
        {
            DebugContract.Requires(services);
            return CreateSchema(services);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a schema.
        /// </summary>
        /// <param name="container">
        ///  The domain services container.
        /// </param>
        /// <returns>
        ///  The new schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ISchema CreateSchema(IServicesContainer container)
        {
            return new Hyperstore.Modeling.Metadata.DomainSchema(_name, container, Behavior);
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