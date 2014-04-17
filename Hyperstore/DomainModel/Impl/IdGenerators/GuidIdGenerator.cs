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
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling.Domain
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A unique identifier generator.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.HyperGraph.IIdGenerator"/>
    ///-------------------------------------------------------------------------------------------------
    public class GuidIdGenerator : IIdGenerator
    {
        private IDomainModel _domainModel;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Nexts the value.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Identity NextValue(ISchemaElement schemaElement)
        {
            return new Identity(_domainModel.Name, Guid.NewGuid()
                    .ToString("N"));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets the specified identifier.
        /// </summary>
        /// <param name="id">
        ///  The identifier to set.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Set(Identity id)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current value.
        /// </summary>
        /// <value>
        ///  The current value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string CurrentValue
        {
            get { return null; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a domain.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);
            _domainModel = domainModel;
        }
    }
}