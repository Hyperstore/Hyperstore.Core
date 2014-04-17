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

using Hyperstore.Modeling.MemoryStore;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Noeud du graphe pouvant représenter un noeud simple et une relation.
    /// </summary>
    /// <seealso cref="T:ICloneable{IGraphNode}"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IGraphNode 
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the graph.
        /// </summary>
        /// <value>
        ///  The identifier of the graph.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string GraphId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity Id { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity SchemaId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start.
        /// </summary>
        /// <value>
        ///  The identifier of the start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity StartId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity StartSchemaId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end.
        /// </summary>
        /// <value>
        ///  The identifier of the end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity EndId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity EndSchemaId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        object Value { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the type of the node.
        /// </summary>
        /// <value>
        ///  The type of the node.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        NodeType NodeType { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the edges.
        /// </summary>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <returns>
        ///  The edges.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Collections.Generic.IEnumerable<EdgeInfo> GetEdges(Direction direction, ISchemaRelationship schemaRelationship);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an edge.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metadataId">
        ///  Identifier for the metadata.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddEdge(Identity id, Identity metadataId, Direction direction);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the edge.
        /// </summary>
        /// <param name="edgeId">
        ///  Identifier for the edge.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RemoveEdge(Identity edgeId, Direction direction);
    } 
}