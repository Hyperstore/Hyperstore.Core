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
        ///  (Optional) The version (corresponding at UtcNow.Ticks)
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ChangePropertyValueCommand(IModelElement element, ISchemaProperty propertySchema, object value, long? version = null)
            : base(element.DomainModel, version)
        {
            Contract.Requires(element, "element");
            Contract.Requires(propertySchema, "propertySchema");

            Value = value;
            Element = element;
            SchemaProperty = propertySchema;
        }

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
        ///  Gets or sets the old version.
        /// </summary>
        /// <value>
        ///  The old version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long OldVersion { get; private set; }

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
                if (pv == null)
                    return null;
                OldValue = pv.OldValue;
                OldVersion = pv.CurrentVersion;
            }

            var evt = new ChangePropertyValueEvent(Element.DomainModel.Name,
                                                   DomainModel.ExtensionName,
                                                   Element.Id,
                                                   Element.SchemaInfo.Id,
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