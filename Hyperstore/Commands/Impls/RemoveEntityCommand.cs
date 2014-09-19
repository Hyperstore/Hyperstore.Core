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
    ///  A remove entity command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.RemoveEntityCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class RemoveEntityCommand : PrimitiveCommand, ICommandHandler<RemoveEntityCommand>
    {
        private readonly bool _throwExceptionIfNotExists;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="RemoveEntityCommand" /> class.
        /// </summary>
        /// <param name="entity">
        ///  The mel.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemoveEntityCommand(IModelEntity entity, bool throwExceptionIfNotExists = true, long? version = null)
            : base(entity.DomainModel, version)
        {
            Contract.Requires(entity, "entity");
            Entity = entity;
            _throwExceptionIfNotExists = throwExceptionIfNotExists;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="RemoveEntityCommand" /> class.
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
        /// <param name="schemaEntityId">
        ///  The schema entity identifier.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <param name="version">
        ///  (Optional) the version.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RemoveEntityCommand(IDomainModel domainModel, Identity id, Identity schemaEntityId, bool throwExceptionIfNotExists = true, long? version = null)
            : base(domainModel, version)
        {
            Contract.Requires(id, "id");
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(schemaEntityId, "schemaEntityId");

            var metadata = domainModel.Store.GetSchemaEntity(schemaEntityId);
            Entity = domainModel.Store.GetEntity(id, metadata);
            if (Entity == null)
                throw new InvalidElementException(id);
            _throwExceptionIfNotExists = throwExceptionIfNotExists;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <value>
        ///  The element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelEntity Entity { get; private set; }

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
        public IEvent Handle(ExecutionCommandContext<RemoveEntityCommand> context)
        {
            DebugContract.Requires(context);
            var dm = DomainModel as IUpdatableDomainModel;
            if (dm == null)
                return null;

            using (CodeMarker.MarkBlock("RemoveEntityCommand.Handle"))
            {
                if (!dm.RemoveEntity(Entity.Id, (ISchemaEntity)Entity.SchemaInfo, _throwExceptionIfNotExists))
                    return null;
            }
            return new RemoveEntityEvent(DomainModel.Name, DomainModel.ExtensionName, Entity.Id, Entity.SchemaInfo.Id, context.CurrentSession.SessionId, Version.Value);
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
            return String.Format("Remove entity '{0}'", Entity.Id);
        }
    }
}