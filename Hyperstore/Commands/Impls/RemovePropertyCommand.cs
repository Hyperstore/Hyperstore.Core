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
        /// <param name="schemaElement">
        ///  The container schema.
        /// </param>
        /// <param name="propertySchema">
        ///  The property schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemovePropertyCommand(IDomainModel domainModel, Identity ownerId, ISchemaElement schemaElement, ISchemaProperty propertySchema)
            : base(domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(schemaElement, "schemaElement");
            Contract.Requires(ownerId, "ownerId");
            Contract.Requires(propertySchema, "propertySchema");

            ElementId = ownerId;
            SchemaElement = schemaElement;
            SchemaProperty = propertySchema;
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
        ///  Gets the schema container.
        /// </summary>
        /// <value>
        ///  The schema container.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement SchemaElement { get; private set; }

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
                var pv = DomainModel.GetPropertyValue(ElementId, SchemaElement, SchemaProperty);
                if (pv != null)
                {
                    OldValue = pv.OldValue;
                    OldVersion = pv.CurrentVersion;
                    return new RemovePropertyEvent(DomainModel.Name, DomainModel.ExtensionName, ElementId, SchemaElement.Id, SchemaProperty.Id, SchemaProperty.Name, SchemaProperty.PropertySchema.Serialize(OldValue), context.CurrentSession.SessionId, OldVersion);
                }
            }
            return null;
        }
    }
}