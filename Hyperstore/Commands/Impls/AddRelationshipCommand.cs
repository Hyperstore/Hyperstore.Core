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

        public AddRelationshipCommand(ISchemaRelationship relationshipSchema, IModelElement start, IModelElement end,
                              Identity id = null, long? version = null)
            : this(relationshipSchema, start, end.Id, id, version)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddRelationshipCommand" /> class.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public AddRelationshipCommand(ISchemaRelationship relationshipSchema, IModelElement start, Identity endId, 
                                      Identity id = null, long? version = null)
            : base(start.DomainModel, version)
        {
            Contract.Requires(start, "start");
            Contract.Requires(endId, "endId");
            Contract.Requires(endId, "endId");
            Contract.Requires(relationshipSchema, "relationshipSchema");

            Start = start;
            EndId = endId;
            _domainModel = start.DomainModel;
            Id = id ?? DomainModel.IdGenerator.NextValue(relationshipSchema);
            if (String.Compare(Id.DomainModelName, start.DomainModel.Name, StringComparison.OrdinalIgnoreCase) != 0)
                throw new InvalidIdException("The id must be an id of the specified domain model.");

            if (relationshipSchema.IsEmbedded && start.Id == endId)
                throw new CircularReferenceException("An element can not contain itself.");

            SchemaRelationship = relationshipSchema;
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
        ///  Gets the start element.
        /// </summary>
        /// <value>
        ///  The start element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement Start { get; private set; }


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
        ///  Gets the relation ship.
        /// </summary>
        /// <value>
        ///  The relation ship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship Relationship
        {
            get { return _element ?? (_element = DomainModel.GetRelationship(Id)); }
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

            if (Start == null)
                new InvalidElementException(Start.Id, "Source element must exists to create a relationship");

            if (String.Compare(Start.Id.DomainModelName, EndId.DomainModelName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (DomainModel.Store.GetElement(EndId) == null)
                    throw new InvalidElementException(EndId, "Target element must exists to create a relationship.");
            }

            using (CodeMarker.MarkBlock("AddRelationshipCommand.Handle"))
            {
                dm.CreateRelationship(Id, SchemaRelationship, Start, EndId, _element);
            }

            return new AddRelationshipEvent(_domainModel.Name, DomainModel.ExtensionName, Id, SchemaRelationship.Id, Start.Id, EndId, context.CurrentSession.SessionId, Version.Value);
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
            return String.Format("Add '{0}--[{2}]->{1}", Start.Id, EndId, Id);
        }
    }
}