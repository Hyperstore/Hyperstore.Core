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

using System.Diagnostics;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Stockage des informations d'un edge au niveau d'un vertex.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("Id={Id}, SchemaId={SchemaId}")]
    public class EdgeInfo : Hyperstore.Modeling.HyperGraph.NodeInfo 
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaId">
        ///  The identifier of the meta class.
        /// </param>
        /// <param name="endId">
        ///  The end identifier.
        /// </param>
        /// <param name="endSchemaId">
        ///  The end schema identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EdgeInfo(Identity id, Identity schemaId, Identity endId, Identity endSchemaId) : base(id, schemaId)
        {
            Contract.Requires(id, "id");
            Contract.Requires(schemaId, "schemaId");
            Contract.Requires(endId, "endId");
            Contract.Requires(endSchemaId, "endSchemaId");

            EndId = endId;
            EndSchemaId = endSchemaId;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the end.
        /// </summary>
        /// <value>
        ///  The identifier of the end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the end schema.
        /// </summary>
        /// <value>
        ///  The identifier of the end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndSchemaId { get; private set; }
    }
}