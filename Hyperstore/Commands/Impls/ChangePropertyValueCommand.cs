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
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A change property value command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.ChangePropertyValueCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ChangePropertyValueCommand : PrimitiveCommand, ICommandHandler<ChangePropertyValueCommand>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="ChangePropertyValueCommand" /> class.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        /// <param name="propertySchema">
        ///  The property schema.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="version">
        ///  (Optional) The version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ChangePropertyValueCommand(IModelElement element, ISchemaProperty propertySchema, object value, long? version=null)
            : base(element.DomainModel)
        {
            Contract.Requires(element, "element");
            Contract.Requires(propertySchema, "propertySchema");

            Value = value;
            Element = element;
            SchemaProperty = propertySchema;
            Version = version;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the version.
        /// </summary>
        /// <value>
        ///  The version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long? Version { get; set; }

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
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Value { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the owner.
        /// </summary>
        /// <value>
        ///  The owner.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement Element { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema property.
        /// </summary>
        /// <value>
        ///  The schema property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaProperty SchemaProperty { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the given context.
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An IEvent.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEvent Handle(ExecutionCommandContext<ChangePropertyValueCommand> context)
        {
            DebugContract.Requires(context);
            var dm = DomainModel as IUpdatableDomainModel;
            if (dm == null)
                return null;

            using (CodeMarker.MarkBlock("ChangeAttributeCommand.Handler"))
            {
                var pv = dm.SetPropertyValue(Element, SchemaProperty, Value, this.Version);
                OldValue = pv.OldValue;
                Version = pv.CurrentVersion;
            }

            var evt = new ChangePropertyValueEvent(Element.DomainModel.Name, 
                                                   DomainModel.ExtensionName, 
                                                   Element.Id, 
                                                   Element.SchemaInfo.Id, 
                                                   SchemaProperty.Id, 
                                                   SchemaProperty.Name, 
                                                   SchemaProperty.PropertySchema.Serialize(Value), 
                                                   SchemaProperty.PropertySchema.Serialize(OldValue),
                                                   context.CurrentSession.SessionId, 
                                                   Version.Value);
            evt.SetInternalValue(Value, OldValue);
            return evt;
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
            return String.Format("Change {0}.{1} = {2}", Element.Id, SchemaProperty.Name, Value);
        }
    }
}