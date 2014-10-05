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
    ///  A change property value event.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.DomainEvent"/>
    /// <seealso cref="T:Hyperstore.Modeling.Events.IUndoableEvent"/>
    ///-------------------------------------------------------------------------------------------------
    public class ChangePropertyValueEvent : AbstractDomainEvent, IUndoableEvent
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public ChangePropertyValueEvent()
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
        /// <param name="elementId">
        ///  The identifier of the element.
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
        /// <param name="value">
        ///  .
        /// </param>
        /// <param name="oldValue">
        ///  .
        /// </param>
        /// <param name="correlationId">
        ///  Identifier for the correlation.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ChangePropertyValueEvent(string domainModelName, string extensionName, Identity elementId, Identity schemaElementId, Identity schemaPropertyId, string propertyName, string value, string oldValue, int correlationId, long version)
                : base(domainModelName, extensionName, version, correlationId)
        {
            Contract.Requires(elementId, "elementId");
            Contract.Requires(schemaElementId, "schemaElementId");
            Contract.Requires(schemaPropertyId, "schemaPropertyId");
            Contract.RequiresNotEmpty(propertyName, "propertyName");

            ElementId = elementId;
            SchemaElementId = schemaElementId;
            SchemaPropertyId = schemaPropertyId;
            PropertyName = propertyName;
            Value = value;
            OldValue = oldValue;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the element.
        /// </summary>
        /// <value>
        ///  The identifier of the element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity ElementId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the schema element.
        /// </summary>
        /// <value>
        ///  The identifier of the schema element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaElementId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the schema property.
        /// </summary>
        /// <value>
        ///  The identifier of the schema property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaPropertyId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Value { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the old value.
        /// </summary>
        /// <value>
        ///  The old value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string OldValue { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the name of the property.
        /// </summary>
        /// <value>
        ///  The name of the property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string PropertyName { get; set; }

        internal object InternalValue { get; private set; }
        internal object InternalOldValue { get; private set; }

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
        public virtual IEvent GetReverseEvent(int correlationId)
        {
            return new ChangePropertyValueEvent(DomainModel, ExtensionName, ElementId, SchemaElementId, SchemaPropertyId, PropertyName, OldValue, Value, correlationId, Version);
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
            return String.Format("Change {0}.{1} = {2} (V{3})", ElementId, PropertyName, Value, Version);
        }

        /// <summary>
        ///     Used to optimize local value propagation (see SessionTrackingData)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="oldValue"></param>
        internal void SetInternalValue(object value, object oldValue)
        {
            InternalValue = value;
            InternalOldValue = OldValue;
        }
    }
}