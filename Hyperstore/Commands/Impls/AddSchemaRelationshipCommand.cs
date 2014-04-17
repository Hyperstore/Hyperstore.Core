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
    ///  An add schema relationship command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.AddSchemaRelationshipCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class AddSchemaRelationshipCommand : PrimitiveCommand, ICommandHandler<AddSchemaRelationshipCommand>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddSchemaRelationshipCommand" /> class.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddSchemaRelationshipCommand(ISchema domainModel, Identity id, ISchemaRelationship schemaRelationship, ISchemaElement start, ISchemaElement end)
            : base(domainModel)
        {
            Contract.Requires(id, "id");
            Contract.Requires(schemaRelationship, "schemaRelationship");
            Contract.Requires(start, "start");
            Contract.Requires(end, "end");

            SchemaRelationship = schemaRelationship;
            Id = id;
            Start = start;
            End = end;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship.
        /// </summary>
        /// <value>
        ///  The schema relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship SchemaRelationship { get; private set; }

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
        ///  Gets the start.
        /// </summary>
        /// <value>
        ///  The start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement Start { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end.
        /// </summary>
        /// <value>
        ///  The end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement End { get; private set; }

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
        public IEvent Handle(ExecutionCommandContext<AddSchemaRelationshipCommand> context)
        {
            DebugContract.Requires(context);
            var updatableMetaModel = DomainModel as IUpdatableSchema;
            if (updatableMetaModel == null)
                throw new ReadOnlyException("Read only schema");

            using (CodeMarker.MarkBlock("AddRelationshipSchemaCommand.Handle"))
            {
                updatableMetaModel.AddRelationshipSchema(Id, SchemaRelationship, Start, End);
            }

            return new AddSchemaRelationshipEvent(DomainModel.Name, DomainModel.ExtensionName, Id, SchemaRelationship.Id, Start.Id, Start.SchemaInfo.Id, End.Id, End.SchemaInfo.Id, context.CurrentSession.SessionId, 1);
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
            return String.Format("Add relationship schema '{0}')", Id);
        }
    }
}