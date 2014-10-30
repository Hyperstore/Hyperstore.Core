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
    ///  An add entity event.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.DomainEvent"/>
    /// <seealso cref="T:Hyperstore.Modeling.Events.IUndoableEvent"/>
    ///-------------------------------------------------------------------------------------------------
    public class AddEntityEvent : AbstractDomainEvent, IUndoableEvent
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public AddEntityEvent()
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
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaEntityId">
        ///  The identifier of the schema entity.
        /// </param>
        /// <param name="correlationId">
        ///  Identifier for the correlation.
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddEntityEvent(string domainModelName, string extensionName, Identity id, Identity schemaEntityId, int correlationId, long version)
            : base(domainModelName, extensionName, version, correlationId)
        {
            Contract.Requires(id, "id");
            Contract.Requires(schemaEntityId, "schemaEntityId");

            Id = id;
            SchemaId = schemaEntityId;
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
        ///  Gets or sets the identifier of the schema entity.
        /// </summary>
        /// <value>
        ///  The identifier of the schema entity.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get; set; }

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
            return new RemoveEntityEvent(Domain, ExtensionName, Id, SchemaId, correlationId, Version);
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
            return String.Format("Add entity {0} [{1}]", Id, SchemaId);
        }
    }
}