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
    ///  A change property value event.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.DomainEvent"/>
    /// <seealso cref="T:Hyperstore.Modeling.Events.IUndoableEvent"/>
    ///-------------------------------------------------------------------------------------------------
    public class ChangePropertyValueEvent : DomainEvent, IUndoableEvent
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
        public ChangePropertyValueEvent(string domainModelName, string extensionName, Identity elementId, Identity schemaElementId, Identity schemaPropertyId, string propertyName, string value, string oldValue, Guid correlationId, long version)
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
        public virtual IEvent GetReverseEvent(Guid correlationId)
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