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

using System.Collections.Generic;
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling.Events
{
    internal class RemoveRelationshipEventHandler : IEventHandler<RemoveRelationshipEvent>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates handle in this collection.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="event">
        ///  The event.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process handle in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IDomainCommand> Handle(IDomainModel domainModel, RemoveRelationshipEvent @event)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(@event, "@event");

            var metadata = domainModel.Store.GetSchemaRelationship(@event.SchemaId);
            var mel = domainModel.GetRelationship(@event.Id, metadata);
            if (mel != null)
                yield return new RemoveRelationshipCommand(mel);
        }
    }
}