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
    ///  A remove property event.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.DomainEvent"/>
    /// <seealso cref="T:Hyperstore.Modeling.Events.IUndoableEvent"/>
    ///-------------------------------------------------------------------------------------------------
    public class RemovePropertyEvent : AbstractDomainEvent, IUndoableEvent
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public RemovePropertyEvent()
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
        /// <param name="ownerId">
        ///  The identifier that owns this item.
        /// </param>
        /// <param name="schemaElementId">
        ///  The identifier of the schema element.
        /// </param>
        /// <param name="schemaPropertyId">
        ///  The identifier of the schema property.
        /// </param>
        /// <param name="propertyName">
        ///  The name of the property.
        /// </param>
        /// <param name="oldValue">
        ///  The old value.
        /// </param>
        /// <param name="correlationId">
        ///  Identifier for the correlation.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemovePropertyEvent(string domainModelName, string extensionName, Identity ownerId, Identity schemaElementId, string propertyName, object oldValue, int correlationId, long version)
            : base(domainModelName, extensionName, version, correlationId)
        {
            Contract.Requires(ownerId, "ownerId");
            Contract.Requires(schemaElementId, "schemaElementId");
            Contract.RequiresNotEmpty(propertyName, "propertyName");

            OldValue = oldValue;
            Id = ownerId;
            SchemaId = schemaElementId;
            PropertyName = propertyName;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the element.
        /// </summary>
        /// <value>
        ///  The identifier of the element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the schema element.
        /// </summary>
        /// <value>
        ///  The identifier of the schema element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the name of the property.
        /// </summary>
        /// <value>
        ///  The name of the property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string PropertyName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the old value.
        /// </summary>
        /// <value>
        ///  The old value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object OldValue { get; set; }

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
            return new ChangePropertyValueEvent(Domain, ExtensionName, Id, SchemaId, PropertyName, OldValue, null, correlationId, Version);
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
            return String.Format("Remove property {0}.{1}", Id, PropertyName);
        }
    }
}