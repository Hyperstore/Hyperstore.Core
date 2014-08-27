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
using Hyperstore.Modeling.HyperGraph;
using System;

#endregion

namespace Hyperstore.Modeling.Adapters
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Graph node including all its properties
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public class GraphPropertiesNode : GraphNode
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a graph properties node.
        /// </summary>
        /// <param name="nodeType">
        ///  The type of the node.
        /// </param>
        /// <param name="id">
        ///  The node identifier.
        /// </param>
        /// <param name="startId">
        ///  For a relationship, the start node id.
        /// </param>
        /// <param name="startSchemaId">
        ///  For a relationship, the start schema id.
        /// </param>
        /// <param name="endId">
        ///  For a relationship, the end node id.
        /// </param>
        /// <param name="endSchemaId">
        ///  For a relationship, the end schema id.
        /// </param>
        /// <param name="schema">
        ///  Schema of the node.
        /// </param>
        /// <param name="props">
        ///  Node properties.
        /// </param>
        /// <param name="outgoings">
        ///  The outgoings relationships of the node
        /// </param>
        /// <param name="incomings">
        ///  The incomings relationships of the node
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public GraphPropertiesNode(NodeType nodeType, Identity id, Identity startId, Identity startSchemaId, 
                                Identity endId, Identity endSchemaId, ISchemaElement schema, Dictionary<ISchemaProperty, PropertyValue> props,
                                IEnumerable<EdgeInfo> outgoings, IEnumerable<EdgeInfo> incomings
            )
            : base( id, schema.Id, nodeType, startId, startSchemaId, endId, endSchemaId, outgoings:outgoings, incomings:incomings)
        {
            Contract.Requires(id != null, "id");
            Contract.Requires(schema != null, "schema");
            SchemaInfo = schema;
            Properties = props ?? new Dictionary<ISchemaProperty, PropertyValue>();            
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Schema of the node
        /// </summary>
        /// <value>
        ///  Information describing the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement SchemaInfo { get; private set; }

        
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Associated properties of the current node.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public IDictionary<ISchemaProperty, PropertyValue> Properties { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an edge.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metadataId">
        ///  Identifier for the metadata.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <param name="endId">
        ///  The identifier of the end.
        /// </param>
        /// <param name="endSchemaId">
        ///  The identifier of the end schema.
        /// </param>
        /// <returns>
        ///  A GraphNode.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override GraphNode AddEdge(Identity id, Identity metadataId, Direction direction, Identity endId, Identity endSchemaId)
        {
            throw new NotImplementedException("Uses constructor instead");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the edge.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <returns>
        ///  A GraphNode.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override GraphNode RemoveEdge(Identity id, Direction direction)
        {
            throw new NotImplementedException("Uses constructor instead");
        }
    }
}