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
    ///  An add relationship command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.AddRelationshipCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class AddRelationshipCommand : PrimitiveCommand, ICommandHandler<AddRelationshipCommand>
    {
        private readonly IDomainModel _domainModel;
        private readonly IModelElement _start;
        private IModelRelationship _element;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddRelationshipCommand" /> class.
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        internal AddRelationshipCommand(IModelRelationship relationship)
            : this(relationship.SchemaInfo as ISchemaRelationship, relationship.Start, relationship.End, relationship.Id)
        {
            Contract.Requires(relationship, "relationship");

            _element = relationship;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddRelationshipCommand" /> class.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="relationshipSchema">
        ///  The relationship schema.
        /// </param>
        /// <param name="startId">
        ///  The start identifier.
        /// </param>
        /// <param name="startSchema">
        ///  The start schema.
        /// </param>
        /// <param name="endId">
        ///  The end identifier.
        /// </param>
        /// <param name="endSchema">
        ///  The end schema.
        /// </param>
        /// <param name="id">
        ///  (Optional) The identifier.
        /// </param>
        /// ### <exception cref="System.Exception">
        ///  .
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        public AddRelationshipCommand(IDomainModel domainModel, ISchemaRelationship relationshipSchema, Identity startId, ISchemaElement startSchema, Identity endId, ISchemaElement endSchema, Identity id = null)
            : base(domainModel)
        {
            Contract.Requires(startId, "startId");
            Contract.Requires(endId, "endId");
            Contract.Requires(startSchema, "startSchema");
            Contract.Requires(endId, "endId");
            Contract.Requires(endSchema, "endSchema");
            Contract.Requires(relationshipSchema, "relationshipSchema");
            Contract.Requires(domainModel, "domainModel");

            if (!startSchema.IsA(relationshipSchema.Start))
                throw new Exception(string.Format(ExceptionMessages.TypeMismatchStartElementMustBeAFormat, relationshipSchema.Start.Name));

            if (!endSchema.IsA(relationshipSchema.End))
                throw new Exception(string.Format(ExceptionMessages.TypeMismatchEndElementMustBeAFormat, relationshipSchema.End.Name));

            StartId = startId;
            EndId = endId;
            StartSchema = startSchema;
            EndSchema = endSchema;
            _domainModel = domainModel;
            Id = id ?? DomainModel.IdGenerator.NextValue(relationshipSchema);
            if (String.Compare(Id.DomainModelName, domainModel.Name, StringComparison.OrdinalIgnoreCase) != 0)
                throw new Exception("The id must be an id of the specified domain model.");

            SchemaRelationship = relationshipSchema;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddRelationshipCommand" /> class.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="relationshipSchema">
        ///  The relationship schema.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="id">
        ///  (Optional) The identifier.
        /// </param>
        /// ### <exception cref="System.Exception">
        ///  .
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        public AddRelationshipCommand(ISchemaRelationship relationshipSchema, IModelElement start, IModelElement end, Identity id = null)
            : this(start.DomainModel, relationshipSchema, start.Id, start.SchemaInfo, end.Id, end.SchemaInfo, id)
        {
            Contract.Requires(start, "start");
            Contract.Requires(end, "end");
            Contract.Requires(relationshipSchema, "relationshipSchema");

            if (start.Status == ModelElementStatus.Disposed)
                throw new Exception(ExceptionMessages.StartElementIsNotAValidElement);

            if (end.Status == ModelElementStatus.Disposed)
                throw new Exception(ExceptionMessages.EndElementIsNotAValidElement);

            _start = start;
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
        ///  Gets the schema relationship.
        /// </summary>
        /// <value>
        ///  The schema relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship SchemaRelationship { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start identifier.
        /// </summary>
        /// <value>
        ///  The start identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start schema.
        /// </summary>
        /// <value>
        ///  The start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement StartSchema { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end identifier.
        /// </summary>
        /// <value>
        ///  The end identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end schema.
        /// </summary>
        /// <value>
        ///  The end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement EndSchema { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relation ship.
        /// </summary>
        /// <value>
        ///  The relation ship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship Relationship
        {
            get { return _element ?? (_element = DomainModel.GetRelationship(Id, SchemaRelationship)); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the given context.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An IEvent.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEvent Handle(ExecutionCommandContext<AddRelationshipCommand> context)
        {
            DebugContract.Requires(context);
            var dm = DomainModel as IUpdatableDomainModel;
            if (dm == null)
                return null;

            var start = _start ?? DomainModel.Store.GetElement(StartId, StartSchema);
            if (start == null)
                throw new InvalidElementException(StartId, "Source element must exists to create a relationship");

            using (CodeMarker.MarkBlock("AddRelationshipCommand.Handle"))
            {
                dm.CreateRelationship(Id, SchemaRelationship, start, EndId, EndSchema, _element);
            }

            return new AddRelationshipEvent(_domainModel.Name, DomainModel.ExtensionName, Id, SchemaRelationship.Id, StartId, StartSchema.Id, EndId, EndSchema.Id, context.CurrentSession.SessionId, 1);
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
            return String.Format("Add '{0}--[{2}]->{1}", StartId, EndId, Id);
        }
    }
}