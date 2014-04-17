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

#endregion

namespace Hyperstore.Modeling.Events
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Un événement portant sur les metadata n'est pas undoable.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.DomainEvent"/>
    ///-------------------------------------------------------------------------------------------------
    public class AddSchemaPropertyEvent : DomainEvent
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public AddSchemaPropertyEvent()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddSchemaPropertyEvent" /> class.
        /// </summary>
        /// <param name="domainModelName">
        ///  Name of the domain model.
        /// </param>
        /// <param name="extensionName">
        ///  Name of the extension.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaPropertyId">
        ///  The schema property identifier.
        /// </param>
        /// <param name="correlationId">
        ///  The correlation identifier.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddSchemaPropertyEvent(string domainModelName, string extensionName, Identity id, Identity schemaPropertyId, Guid correlationId, long version) 
            : base(domainModelName, extensionName, version, correlationId)
        {
            Contract.Requires(id, "id");
            Contract.Requires(schemaPropertyId, "schemaPropertyId");

            Id = id;
            SchemaPropertyId = schemaPropertyId;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the schema property identifier.
        /// </summary>
        /// <value>
        ///  The schema property identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaPropertyId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("Add schema property {0}", Id);
        }
    }
}