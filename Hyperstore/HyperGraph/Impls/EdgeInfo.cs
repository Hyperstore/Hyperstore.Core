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
    public class EdgeInfo : Hyperstore.Modeling.HyperGraph.INodeInfo 
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaClassId">
        ///  The identifier of the meta class.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EdgeInfo(Identity id, Identity metaClassId, Identity endId, Identity endSchemaId)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaClassId, "metaClassId");
            Contract.Requires(endId, "endId");
            Contract.Requires(endSchemaId, "endSchemaId");

            Id = id;
            SchemaId = metaClassId;
            EndId = endId;
            EndSchemaId = endSchemaId;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the schema.
        /// </summary>
        /// <value>
        ///  The identifier of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Identifiant de la relation.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }

        /////-------------------------------------------------------------------------------------------------
        ///// <summary>
        /////  Identifiant du noeud sortant. Ceci permet d'optimiser les traversées de graphe en èvitant de
        /////  lire la relation pour naviguer de noeud en noeud.
        ///// </summary>
        ///// <value>
        /////  The identifier of the end.
        ///// </value>
        /////-------------------------------------------------------------------------------------------------
        public Identity EndId { get; private set; }

        /////-------------------------------------------------------------------------------------------------
        ///// <summary>
        /////  Gets the identifier of the end schema.
        ///// </summary>
        ///// <value>
        /////  The identifier of the end schema.
        ///// </value>
        /////-------------------------------------------------------------------------------------------------
        public Identity EndSchemaId { get; private set; }
    }
}