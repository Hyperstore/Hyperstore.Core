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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling
{
    public static class DomainExtensions
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  An IHyperstore extension method that creates an entity.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///  Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="store">
        ///  The store to act on.
        /// </param>
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
        /// <exception cref="ArgumentException">
        ///  Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="store">
        ///  The store to act on.
        /// </param>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="domainModel">
        ///  (Optional) the domain model.
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
