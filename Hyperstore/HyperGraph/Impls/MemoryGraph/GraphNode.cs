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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hyperstore.Modeling.MemoryStore;
using System.Collections.Immutable;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Represent an immutable graph node which can be a simple node or a relationship
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.HyperGraph.GraphNode"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class GraphNode : EdgeInfo
    {
        private readonly ImmutableDictionary<Identity, EdgeInfo> _incomings = ImmutableDictionary<Identity, EdgeInfo>.Empty;
        private readonly ImmutableDictionary<Identity, EdgeInfo> _outgoings = ImmutableDictionary<Identity, EdgeInfo>.Empty;

        private string DebuggerDisplayString
        {
            get { return String.Format("Id={0}, Start={1}, End={2}, Value={3}", Id, StartId, EndId, Value); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaId">
        ///  Identifier for the meta class.
        /// </param>
        /// <param name="nodetype">
        ///  The type of the node.
        /// </param>
        /// <param name="start">
        ///  (Optional) the start.
        /// </param>
        /// <param name="startSchema">
        ///  (Optional) the start metaclass.
        /// </param>
        /// <param name="end">
        ///  (Optional) the end.
        /// </param>
        /// <param name="endSchema">
        ///  (Optional) the end metaclass.
        /// </param>
        /// <param name="value">
        ///  (Optional) The value.
        /// </param>
        /// <param name="version">
        ///  (Optional) The version.
        /// </param>
        /// <param name="outgoings">
        ///  (Optional) The outgoings.
        /// </param>
        /// <param name="incomings">
        ///  (Optional) The incomings.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        internal GraphNode(Identity id, Identity schemaId, NodeType nodetype,
                            Identity start = null,
                            Identity end = null, 
                            object value = null, long? version = null,
                            IEnumerable<EdgeInfo> outgoings = null, IEnumerable<EdgeInfo> incomings = null
            )
            : base(id, schemaId, end)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(schemaId, "schemaId");
            DebugContract.Requires(start == null || end != null, "start/end");

            StartId = start;

            Value = value;
            Version = version ?? DateTime.UtcNow.Ticks;
            NodeType = nodetype;

            if (outgoings != null)
                _outgoings = _outgoings.AddRange(outgoings.ToDictionary(e => e.Id));
            if (incomings != null)
                _incomings = _incomings.AddRange(incomings.ToDictionary(e => e.Id));
        }

        private GraphNode(GraphNode copy)
            : this(copy.Id, copy.SchemaId, copy.NodeType, copy.StartId, copy.EndId, copy.Value, outgoings: copy.Outgoings, incomings: copy.Incomings)
        {
            DebugContract.Requires(copy, "copy");
        }

        private GraphNode(GraphNode copy, ImmutableDictionary<Identity, EdgeInfo> outgoings, ImmutableDictionary<Identity, EdgeInfo> incomings)
            : this(copy)
        {
            _outgoings = outgoings;
            _incomings = incomings;
        }

        private GraphNode(GraphNode copy, object value)
            : this(copy)
        {
            DebugContract.Requires(copy, "copy");
            Value = value;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start.
        /// </summary>
        /// <value>
        ///  The identifier of the start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartId { get; private set; }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the incomings.
        /// </summary>
        /// <value>
        ///  The incomings.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<EdgeInfo> Incomings { get { return _incomings.Values; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the outgoings.
        /// </summary>
        /// <value>
        ///  The outgoings.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<EdgeInfo> Outgoings { get { return _outgoings.Values; } }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Value { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the version.
        /// </summary>
        /// <value>
        ///  The version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long Version { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a value.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <returns>
        ///  A MemoryGraphNode.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public GraphNode SetValue(object value)
        {
            return new GraphNode(this, value);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an edge.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
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
        /// <param name="endSchemaId">
        ///  The identifier of the end schema.
        /// </param>
        /// <returns>
        ///  A GraphNode.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual GraphNode AddEdge(Identity id, ISchemaRelationship schemaRelationship, Direction direction, Identity endId)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(schemaRelationship, "schemaRelationship");
            DebugContract.Requires(endId, "endId");

            if ((direction & Direction.Outgoing) == Direction.Outgoing && _outgoings.ContainsKey(id)
                || (direction & Direction.Incoming) == Direction.Incoming && _incomings.ContainsKey(id))
                throw new DuplicateElementException("Duplicate relationship");

            // Check multi containers
            // TODO no it's valid if they are not all set (make a test in the command ???)
            //if ((direction & Direction.Incoming) == Direction.Incoming && schemaRelationship.IsEmbedded)
            //{
            //    if (_incomings.Any(r => r.Value.SchemaId == schemaRelationship.Id))
            //        return null;
            //}
            var edge = new EdgeInfo(id, schemaRelationship.Id, endId);

            return new GraphNode(this,
                    (direction & Direction.Outgoing) == Direction.Outgoing ? _outgoings.Add(id, edge) : _outgoings,
                    (direction & Direction.Incoming) == Direction.Incoming ? _incomings.Add(id, edge) : _incomings
                );
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the edge.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public virtual GraphNode RemoveEdge(Identity id, Direction direction)
        {
            DebugContract.Requires(id, "id");

            if ((direction & Direction.Outgoing) == Direction.Outgoing && !_outgoings.ContainsKey(id)
                || (direction & Direction.Incoming) == Direction.Incoming && !_incomings.ContainsKey(id))
                return this;

            return new GraphNode(this,
                    (direction & Direction.Outgoing) == Direction.Outgoing ? _outgoings.Remove(id) : _outgoings,
                    (direction & Direction.Incoming) == Direction.Incoming ? _incomings.Remove(id) : _incomings
                );
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the type of the node.
        /// </summary>
        /// <value>
        ///  The type of the node.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public NodeType NodeType
        {
            get;
            private set;
        }
    }
}