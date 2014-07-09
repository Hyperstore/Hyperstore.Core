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
    ///  A remove relationship event.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.DomainEvent"/>
    /// <seealso cref="T:Hyperstore.Modeling.Events.IUndoableEvent"/>
    ///-------------------------------------------------------------------------------------------------
    public class RemoveRelationshipEvent : AbstractDomainEvent, IUndoableEvent
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public RemoveRelationshipEvent()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModelName">
        ///  Name of the domain model.
        /// </param>
        /// <param name="extensionName">
        ///  Name of the extension.
        /// </param>
        /// <param name="relationshipId">
        ///  The identifier.
        /// </param>
        /// <param name="schemaRelationshipId">
        ///  The identifier of the schema relationship.
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
        /// <param name="correlationId">
        ///  Identifier for the correlation.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemoveRelationshipEvent(string domainModelName, string extensionName, Identity relationshipId, Identity schemaRelationshipId, Identity startId, Identity startSchema, Identity endId, Identity endSchema, Guid correlationId, long version, string propertyName)
                : base(domainModelName, extensionName, version, correlationId)
        {
            Contract.Requires(relationshipId, "relationshipId");
            Contract.Requires(schemaRelationshipId, "schemaRelationshipId");
            Contract.Requires(startId, "startId");
            Contract.Requires(endId, "endId");
            Contract.Requires(startSchema, "startSchema");
            Contract.Requires(endSchema, "endSchema");

            PropertyName = propertyName;
            RelationshipId = relationshipId;
            Start = startId;
            End = endId;
            SchemaRelationshipId = schemaRelationshipId;
            StartSchema = startSchema;
            EndSchema = endSchema;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the start.
        /// </summary>
        /// <value>
        ///  The start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Start { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the start schema.
        /// </summary>
        /// <value>
        ///  The start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartSchema { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the end.
        /// </summary>
        /// <value>
        ///  The end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity End { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the end schema.
        /// </summary>
        /// <value>
        ///  The end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndSchema { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the name of the property if the relationship is a 1..x reference
        /// </summary>
        /// <value>
        ///  The name of the property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string PropertyName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity RelationshipId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the schema relationship.
        /// </summary>
        /// <value>
        ///  The identifier of the schema relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaRelationshipId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the reverse event.
        /// </summary>
        /// <param name="correlationId">
        ///  Identifier for the correlation.
        /// </param>
        /// <returns>
        ///  The reverse event.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEvent GetReverseEvent(Guid correlationId)
        {
            return new AddRelationshipEvent(DomainModel, ExtensionName, RelationshipId, SchemaRelationshipId, Start, StartSchema, End, EndSchema, correlationId, Version, PropertyName);
        }

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
            return String.Format("Remove {0} -[{2}]-> {1}", Start, End, SchemaRelationshipId);
        }
    }
}