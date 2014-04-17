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
using System.Collections.Generic;
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling.Events
{
    internal class AddRelationshipMetadataEventHandler : IEventHandler<AddSchemaRelationshipEvent>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates handle in this collection.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
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
        public IEnumerable<IDomainCommand> Handle(IDomainModel domainModel, AddSchemaRelationshipEvent @event)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(@event, "@event");

            var metadata = domainModel.Store.GetSchemaRelationship(@event.SchemaRelationshipId);
            if (domainModel.GetRelationship(@event.Id, metadata) == null)
            {
                var start = domainModel.Store.GetSchemaElement(@event.Start);
                if (start == null)
                    throw new InvalidElementException( @event.Start);
                var end = domainModel.Store.GetSchemaElement(@event.End);
                if (end == null)
                    throw new InvalidElementException( @event.End);

                yield return new AddSchemaRelationshipCommand(domainModel as ISchema, @event.Id, metadata, start, end);
            }
        }
    }
}