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
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for schema definition.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface ISchemaDefinition : IDomainConfiguration
    {
        DomainBehavior Behavior { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name of the schema.
        /// </summary>
        /// <value>
        ///  The name of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string SchemaName { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Defines the schema.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void DefineSchema(ISchema schema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the schema loaded action.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void OnSchemaLoaded(ISchema schema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Creates a schema. </summary>
        /// <param name="services">   The domain services. </param>
        /// <returns>   The new schema. </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchema CreateSchema(IServicesContainer services);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Loads dependent schemas. </summary>
        /// <param name="store">    The store. </param>
        ///-------------------------------------------------------------------------------------------------
        void LoadDependentSchemas(IHyperstore store);
    }
}