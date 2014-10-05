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
        public RemoveRelationshipEvent(string domainModelName, string extensionName, Identity relationshipId, Identity schemaRelationshipId, Identity startId, Identity startSchema, Identity endId, Identity endSchema, int correlationId, long version)
                : base(domainModelName, extensionName, version, correlationId)
        {
            Contract.Requires(relationshipId, "relationshipId");
            Contract.Requires(schemaRelationshipId, "schemaRelationshipId");
            Contract.Requires(startId, "startId");
            Contract.Requires(endId, "endId");
            Contract.Requires(startSchema, "startSchema");
            Contract.Requires(endSchema, "endSchema");

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
        public IEvent GetReverseEvent(int correlationId)
        {
            return new AddRelationshipEvent(DomainModel, ExtensionName, RelationshipId, SchemaRelationshipId, Start, StartSchema, End, EndSchema, correlationId, Version);
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