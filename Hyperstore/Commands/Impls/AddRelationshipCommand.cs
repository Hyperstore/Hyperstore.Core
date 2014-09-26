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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddRelationshipCommand" /> class.
        /// </summary>
        /// <param name="relationship">
        ///  The relationship.
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        internal AddRelationshipCommand(IModelRelationship relationship, long? version = null)
            : this(relationship.SchemaInfo as ISchemaRelationship, relationship.Start, relationship.End, relationship.Id, version)
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
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddRelationshipCommand(IDomainModel domainModel, ISchemaRelationship relationshipSchema, Identity startId, ISchemaElement startSchema, Identity endId, ISchemaElement endSchema, Identity id = null, long? version = null)
            : base(domainModel, version)
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

            if (relationshipSchema.IsEmbedded && startId == endId)
                throw new Exception("An element can not contain itself.");

            SchemaRelationship = relationshipSchema;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddRelationshipCommand" /> class.
        /// </summary>
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
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddRelationshipCommand(ISchemaRelationship relationshipSchema, IModelElement start, IModelElement end, Identity id = null, long? version = null)
            : this(start.DomainModel, relationshipSchema, start.Id, start.SchemaInfo, end.Id, end.SchemaInfo, id, version)
        {
            Contract.Requires(start, "start");
            Contract.Requires(end, "end");
            Contract.Requires(relationshipSchema, "relationshipSchema");

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

            return new AddRelationshipEvent(_domainModel.Name, DomainModel.ExtensionName, Id, SchemaRelationship.Id, StartId, StartSchema.Id, EndId, EndSchema.Id, context.CurrentSession.SessionId, Version.Value);
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