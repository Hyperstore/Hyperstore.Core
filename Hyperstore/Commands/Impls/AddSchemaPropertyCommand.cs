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
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An add schema property command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.AddSchemaPropertyCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class AddSchemaPropertyCommand : PrimitiveCommand, ICommandHandler<AddSchemaPropertyCommand>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddSchemaPropertyCommand" /> class.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="propertyId">
        ///  The property identifier.
        /// </param>
        /// <param name="propertySchema">
        ///  The property schema.
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddSchemaPropertyCommand(ISchema domainModel, Identity propertyId, ISchemaEntity propertySchema, long? version = null)
            : base(domainModel, version)
        {
            Contract.Requires(propertyId, "propertyId");
            Contract.Requires(propertySchema, "propertySchema");

            PropertyId = propertyId;
            PropertySchema = propertySchema;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property identifier.
        /// </summary>
        /// <value>
        ///  The property identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity PropertyId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property schema.
        /// </summary>
        /// <value>
        ///  The property schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaEntity PropertySchema { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the given context.
        /// </summary>
        /// <exception cref="ReadOnlyException">
        ///  Thrown when a Read Only error condition occurs.
        /// </exception>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An IEvent.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEvent Handle(ExecutionCommandContext<AddSchemaPropertyCommand> context)
        {
            DebugContract.Requires(context);
            var updatableMetaModel = DomainModel as IUpdatableSchema;
            if (updatableMetaModel == null)
                throw new ReadOnlyException("Read only schema");

            using (CodeMarker.MarkBlock("AddPropertySchemaCommand.Handle"))
            {
                updatableMetaModel.AddPropertySchema(PropertyId, PropertySchema);
            }

            return new AddSchemaPropertyEvent(DomainModel.Name, DomainModel.ExtensionName, PropertyId, PropertySchema.Id, context.CurrentSession.SessionId, Version.Value);
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
            return String.Format("Add property schema '{0}' ({1}) )", PropertyId, PropertySchema);
        }
    }
}