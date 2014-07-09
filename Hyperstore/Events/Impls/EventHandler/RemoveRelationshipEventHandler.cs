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

            var metadata = domainModel.Store.GetSchemaRelationship(@event.SchemaRelationshipId);
            var mel = domainModel.GetRelationship(@event.RelationshipId, metadata);
            if (mel != null)
                yield return new RemoveRelationshipCommand(mel, @event.PropertyName);
        }
    }
}