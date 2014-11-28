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
        /// <param name="endId">
        ///  For a relationship, the end node id.
        /// </param>
        /// <param name="schema">
        ///  Schema of the node.
        /// </param>
        /// <param name="props">
        ///  Node properties.
        /// </param>
        /// <param name="outgoings">
        ///  The outgoings relationships of the node.
        /// </param>
        /// <param name="incomings">
        ///  The incomings relationships of the node.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public GraphPropertiesNode(NodeType nodeType, Identity id, Identity startId,
                                Identity endId, ISchemaElement schema, Dictionary<ISchemaProperty, PropertyValue> props,
                                IEnumerable<EdgeInfo> outgoings, IEnumerable<EdgeInfo> incomings
            )
            : base( id, schema.Id, nodeType, startId, endId, outgoings:outgoings, incomings:incomings)
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
        /// <param name="schemaRelationship">
        ///  Identifier for the metadata.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <param name="endId">
        ///  The identifier of the end.
        /// </param>
        /// <returns>
        ///  A GraphNode.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override GraphNode AddEdge(Identity id, ISchemaRelationship schemaRelationship, Direction direction, Identity endId)
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