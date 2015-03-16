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
    ///  A remove relationship command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.RemoveRelationshipCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class RemoveRelationshipCommand : PrimitiveCommand, ICommandHandler<RemoveRelationshipCommand>
    {
        private readonly bool _throwExceptionIfNotExists;
        private readonly Identity _startId;

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
        /// <param name="throwExceptionIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemoveRelationshipCommand(IDomainModel domainModel, Identity id, bool throwExceptionIfNotExists = true, long? version = null)
            : base(domainModel, version)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(id, "id");

            _throwExceptionIfNotExists = throwExceptionIfNotExists;
            Relationship = domainModel.Store.GetRelationship(id);
            if (Relationship == null)
            {
                if (throwExceptionIfNotExists)
                    throw new InvalidElementException(id);
            }
            else
                _startId = Relationship.Start.Id;
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="RemoveRelationshipCommand" /> class.
        /// </summary>
        /// <param name="relationship">
        ///  The relationship.
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemoveRelationshipCommand(IModelRelationship relationship, long? version = null)
            : base(relationship.DomainModel, version)
        {
            Contract.Requires(relationship, "relationship");
            Relationship = relationship;
            _startId = Relationship.Start.Id;
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
            if (dm == null || Relationship == null)
                return null;

            // Le noeud terminal n'existe peut-être pas réellement (si il fait partie d'un autre domaine qui n'est pas chargé)
            // Il faut utiliser directement son id
            var endId = Relationship.EndId;

            // Avant la suppression effective
            var @event = new RemoveRelationshipEvent(Relationship.DomainModel.Name,
                                                     Relationship.DomainModel.ExtensionName,
                                                     Relationship.Id,
                                                     Relationship.SchemaInfo.Id,
                                                     _startId,
                                                     endId,
                                                     context.CurrentSession.SessionId,
                                                     Version.Value);

            using (CodeMarker.MarkBlock("RemoveRelationshipCommand.Handle"))
            {
                if (dm.RemoveRelationship(Relationship.Id, _throwExceptionIfNotExists) == null)
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