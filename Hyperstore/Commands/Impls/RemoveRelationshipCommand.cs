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
    ///  A remove relationship command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.RemoveRelationshipCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class RemoveRelationshipCommand : PrimitiveCommand, ICommandHandler<RemoveRelationshipCommand>
    {
        private readonly bool _throwExceptionIfNotExists;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="RemoveRelationshipCommand" /> class.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  .
        /// </exception>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaRelationshipId">
        ///  The schema relationship identifier.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw exception if not exists].
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemoveRelationshipCommand(IDomainModel domainModel, Identity id, Identity schemaRelationshipId, bool throwExceptionIfNotExists = true)
            : base(domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(id, "id");
            Contract.Requires(schemaRelationshipId, "schemaRelationshipId");

            var metadata = domainModel.Store.GetSchemaRelationship(schemaRelationshipId);
            Relationship = domainModel.Store.GetRelationship(id, metadata);
            if (Relationship == null)
                throw new InvalidElementException(id);
            _throwExceptionIfNotExists = throwExceptionIfNotExists;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="RemoveRelationshipCommand" /> class.
        /// </summary>
        /// <param name="relationship">
        ///  The relationship.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemoveRelationshipCommand(IModelRelationship relationship)
            : base(relationship.DomainModel)
        {
            Contract.Requires(relationship, "relationship");
            Relationship = relationship;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <value>
        ///  The relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship Relationship { get; private set; }

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
        public IEvent Handle(ExecutionCommandContext<RemoveRelationshipCommand> context)
        {
            DebugContract.Requires(context);
            var dm = DomainModel as IUpdatableDomainModel;
            if (dm == null)
                return null;

            // Le noeud terminal n'existe peut-être pas réellement (si il fait partie d'un autre domaine qui n'est pas chargé)
            // Il faut utiliser directement son id
            var endId = Relationship.EndId;
            var endSchemaId = Relationship.EndSchemaId;

            // Avant la suppression effective
            var @event = new RemoveRelationshipEvent(Relationship.DomainModel.Name, 
                                                     Relationship.DomainModel.ExtensionName,
                                                     Relationship.Id, 
                                                     Relationship.SchemaInfo.Id, 
                                                     Relationship.Start.Id, 
                                                     Relationship.Start.SchemaInfo.Id, 
                                                     endId, 
                                                     endSchemaId,
                                                     context.CurrentSession.SessionId, 
                                                     1);

            using (CodeMarker.MarkBlock("RemoveRelationshipCommand.Handle"))
            {
                if (!dm.RemoveRelationship(Relationship.Id, Relationship.SchemaInfo as ISchemaRelationship, _throwExceptionIfNotExists))
                    return null;
            }
            return @event;
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
            return String.Format("Remove relationship '{0}", Relationship.Id);
        }
    }
}