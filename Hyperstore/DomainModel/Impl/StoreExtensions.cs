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
using Hyperstore.Modeling.Events;
// Copyright (c) Alain Metge.  All rights reserved.
// This file is part of Hyperstore.
// 
//  Licensed under the Apache License, Version 2.0 (the "License"); you
//  may not use this file except in compliance with the License. You may
// obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied. See the License for the specific language governing permissions
// and limitations under the License.long with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
using System;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A store extensions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class StoreExtensions
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
        public static T CreateEntity<T>(this IHyperstore store, IDomainModel domainModel = null, Identity id = null) where T : IModelEntity
        {
            Contract.Requires(store != null, "store");
            if (Session.Current == null)
                throw new SessionRequiredException();
            var domain = domainModel ?? store.DefaultSessionConfiguration.DefaultDomainModel;
            if (domain == null)
                throw new ArgumentException("domainModel");
            var schema = store.GetSchemaEntity<T>();
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
        public static IModelEntity CreateEntity(this IHyperstore store, ISchemaEntity schema, IDomainModel domainModel = null, Identity id = null)
        {
            Contract.Requires(store != null, "store");
            Contract.Requires(schema != null, "schema");
            if (Session.Current == null)
                throw new SessionRequiredException();

            var domain = domainModel ?? store.DefaultSessionConfiguration.DefaultDomainModel;
            if (domain == null)
                throw new ArgumentException("domainModel");

            var cmd = new AddEntityCommand(domain, schema, id);
            Session.Current.Execute(cmd);
            return cmd.Entity;
        }

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