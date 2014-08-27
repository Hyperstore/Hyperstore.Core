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
    public class GraphNode : NodeInfo
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
                            Identity start = null, Identity startSchema = null,
                            Identity end = null, Identity endSchema = null,
                            object value = null, long? version = null,
                            IEnumerable<EdgeInfo> outgoings = null, IEnumerable<EdgeInfo> incomings = null
            )
            : base(id, schemaId)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(schemaId, "schemaId");
            DebugContract.Requires(start == null || startSchema != null, "start");
            DebugContract.Requires(end == null || endSchema != null, "end");
            DebugContract.Requires(start == null || end != null, "start/end");

            StartId = start;
            StartSchemaId = startSchema;
            EndId = end;
            EndSchemaId = endSchema;
            Value = value;
            Version = version ?? DateTime.UtcNow.Ticks;
            NodeType = nodetype;

            if (outgoings != null)
                _outgoings = _outgoings.AddRange(outgoings.ToDictionary(e => e.Id));
            if (incomings != null)
                _incomings = _incomings.AddRange(incomings.ToDictionary(e => e.Id));
        }

        private GraphNode(GraphNode copy)
            : this(copy.Id, copy.SchemaId, copy.NodeType, copy.StartId, copy.StartSchemaId, copy.EndId, copy.EndSchemaId, copy.Value, outgoings: copy.Outgoings, incomings: copy.Incomings)
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
        ///  Gets the start meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartSchemaId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end.
        /// </summary>
        /// <value>
        ///  The identifier of the end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndSchemaId { get; private set; }
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
        ///-------------------------------------------------------------------------------------------------
        public virtual GraphNode AddEdge(Identity id, Identity metadataId, Direction direction, Identity endId, Identity endSchemaId)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(metadataId, "metadataId");
            DebugContract.Requires(endId, "endId");
            DebugContract.Requires(endSchemaId, "endSchemaId");

            if ((direction & Direction.Outgoing) == Direction.Outgoing && _outgoings.ContainsKey(id)
                || (direction & Direction.Incoming) == Direction.Incoming && _incomings.ContainsKey(id))
                return this;

            var edge = new EdgeInfo(id, metadataId, endId, endSchemaId);

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