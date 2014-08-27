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
    ///  An add schema entity command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.AddSchemaEntityCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class AddSchemaEntityCommand : PrimitiveCommand, ICommandHandler<AddSchemaEntityCommand>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddSchemaEntityCommand" /> class.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="entitySchema">
        ///  The entity schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddSchemaEntityCommand(ISchema domainModel, Identity id, ISchemaEntity entitySchema, long? version = null)
            : base(domainModel, version)
        {
            Contract.Requires(id, "id");
            Contract.Requires(entitySchema, "entitySchema");

            Id = id;
            SchemaEntity = entitySchema;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <value>
        ///  The schema entity.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaEntity SchemaEntity { get; private set; }

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
        public IEvent Handle(ExecutionCommandContext<AddSchemaEntityCommand> context)
        {
            DebugContract.Requires(context);
            var updatableMetaModel = DomainModel as IUpdatableSchema;
            if (updatableMetaModel == null)
                throw new ReadOnlyException("Read only schema");

            using (CodeMarker.MarkBlock("AddSchemaEntityCommand.Handle"))
            {
                updatableMetaModel.AddEntitySchema(Id, SchemaEntity);
            }

            return new AddSchemaEntityEvent(DomainModel.Name, DomainModel.ExtensionName, Id, SchemaEntity.Id, context.CurrentSession.SessionId, Version.Value);
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
            return String.Format("Add entity schema '{0}' ({1}) )", Id, SchemaEntity);
        }
    }
}