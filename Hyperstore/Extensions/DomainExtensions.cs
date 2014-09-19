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
 
using Hyperstore.Modeling.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A domain extensions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class DomainExtensions
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  An IHyperstore extension method that creates an entity.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="domain">
        ///  (Optional) the domain model.
        /// </param>
        /// <param name="id">
        ///  (Optional) the identifier.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static T CreateEntity<T>(this IDomainModel domain, Identity id = null) where T : IModelEntity
        {
            Contract.Requires(domain != null, "domain");
            if (Session.Current == null)
                throw new SessionRequiredException();

            var schema = domain.Store.GetSchemaEntity<T>();
            var cmd = new AddEntityCommand(domain, schema, id);
            Session.Current.Execute(cmd);
            return (T)cmd.Entity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  An IHyperstore extension method that creates an entity.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <param name="domain">
        ///  the domain model.
        /// </param>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="id">
        ///  (Optional) the identifier.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IModelEntity CreateEntity(this IDomainModel domain, ISchemaEntity schema, Identity id = null)
        {
            Contract.Requires(domain != null, "domain");
            Contract.Requires(schema != null, "schema");
            if (Session.Current == null)
                throw new SessionRequiredException();

            var cmd = new AddEntityCommand(domain, schema, id);
            Session.Current.Execute(cmd);
            return cmd.Entity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  An IDomainModel extension method that creates a relationship.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <param name="domain">
        ///  the domain model.
        /// </param>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="startId">
        ///  The start identifier.
        /// </param>
        /// <param name="startSchema">
        ///  The start schema.
        /// </param>
        /// <param name="endId">
        ///  The end identifier.
        /// </param>
        /// <param name="endSchema">
        ///  The end schema.
        /// </param>
        /// <param name="id">
        ///  (Optional) the identifier.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IModelRelationship CreateRelationship(this IDomainModel domain, ISchemaRelationship schema, Identity startId, ISchemaElement startSchema, Identity endId, ISchemaElement endSchema, Identity id = null)
        {
            Contract.Requires(domain != null, "domain");
            Contract.Requires(schema != null, "schema");
            Contract.Requires(startId != null, "startId");
            Contract.Requires(endId != null, "endId");
            Contract.Requires(startSchema != null, "startSchema");
            Contract.Requires(endSchema != null, "endSchema");
            if (Session.Current == null)
                throw new SessionRequiredException();

            var cmd = new AddRelationshipCommand(domain, schema, startId, startSchema, endId, endSchema, id);
            Session.Current.Execute(cmd);
            return cmd.Relationship;
        }
    }
}
