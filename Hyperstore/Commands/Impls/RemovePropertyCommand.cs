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

using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Commands
{
    /// <summary>
    ///     Commande générée lors de la suppression d'un élément.
    /// </summary>
    /// <remarks>
    ///     Attention, seules les propriétés ayant une valeur seront supprimées.
    /// </remarks>
    internal class RemovePropertyCommand : PrimitiveCommand, ICommandHandler<RemovePropertyCommand>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="RemovePropertyCommand" /> class.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="ownerId">
        ///  The owner identifier.
        /// </param>
        /// <param name="ownerSchemaId">
        ///  The schema identifier that owns this item.
        /// </param>
        /// <param name="propertySchema">
        ///  The property schema.
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemovePropertyCommand(IDomainModel domainModel, Identity ownerId, Identity ownerSchemaId, ISchemaProperty propertySchema, long? version = null)
            : base(domainModel, version)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(ownerId, "ownerId");
            Contract.Requires(propertySchema, "propertySchema");
            Contract.Requires(ownerSchemaId, "ownerSchemaId");
            ElementId = ownerId;
            SchemaProperty = propertySchema;
            SchemaId = ownerSchemaId;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the owner identifier.
        /// </summary>
        /// <value>
        ///  The owner identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity ElementId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the element schema.
        /// </summary>
        /// <value>
        ///  The identifier of the element schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get; private set; }

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
        public long OldVersion { get; set; }

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
        public IEvent Handle(ExecutionCommandContext<RemovePropertyCommand> context)
        {
            DebugContract.Requires(context);
            var dm = DomainModel as IUpdatableDomainModel;
            if (dm == null)
                return null;

            using (CodeMarker.MarkBlock("RemovePropertyCommand.Handle"))
            {
                var pv = DomainModel.GetPropertyValue(ElementId, SchemaProperty);
                if (pv != null)
                {
                    OldValue = pv.Value;
                    OldVersion = pv.CurrentVersion;
                    return new RemovePropertyEvent(DomainModel.Name, DomainModel.ExtensionName, ElementId, SchemaId,  SchemaProperty.Name, OldValue, context.CurrentSession.SessionId, OldVersion);
                }
            }
            return null;
        }
    }
}