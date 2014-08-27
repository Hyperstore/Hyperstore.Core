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
    ///  A memory graph node.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.HyperGraph.IGraphNode"/>
    /// <seealso cref="T:System.IDisposable"/>
    /// <seealso cref="T:Hyperstore.Modeling.MemoryStore.ICloneable{Hyperstore.Modeling.HyperGraph.IGraphNode}"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class MemoryGraphNode : IGraphNode, IDisposable
    {
        private readonly ImmutableDictionary<Identity, EdgeInfo> _incomings = ImmutableDictionary<Identity, EdgeInfo>.Empty;
        private readonly ImmutableDictionary<Identity, EdgeInfo> _outgoings = ImmutableDictionary<Identity, EdgeInfo>.Empty;

        private string DebuggerDisplayString
        {
            get { return String.Format("Id={0}, Start={1}, End={2}, Value={3}", Id, StartId, EndId, Value); }
        }

        /// -------------------------------------------------------------------------------------------------
        ///  <summary>
        ///   Constructor.
        ///  </summary>
        ///  <param name="id">
        ///   The identifier.
        ///  </param>
        ///  <param name="schemaId">
        ///   Identifier for the meta class.
        ///  </param>
        ///  <param name="nodetype">
        ///   The type of the node.
        ///  </param>
        ///  <param name="start">
        ///   (Optional) the start.
        ///  </param>
        ///  <param name="startMetaclass">
        ///   (Optional) the start metaclass.
        ///  </param>
        ///  <param name="end">
        ///   (Optional) the end.
        ///  </param>
        ///  <param name="endMetaclass">
        ///   (Optional) the end metaclass.
        ///  </param>
        ///  <param name="value">
        ///   (Optional) The value.
        ///  </param>
        /// <param name="version">
        ///   (Optional) The version.
        ///  </param>
        /// -------------------------------------------------------------------------------------------------
        public MemoryGraphNode(Identity id, Identity schemaId, NodeType nodetype, Identity start = null, Identity startMetaclass = null, Identity end = null, Identity endMetaclass = null, object value = null, long? version = null)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(schemaId, "schemaId");
            DebugContract.Requires(start == null || startMetaclass != null, "start");
            DebugContract.Requires(end == null || endMetaclass != null, "end");
            DebugContract.Requires(start == null || end != null, "start/end");

            StartId = start;
            EndId = end;
            StartSchemaId = startMetaclass;
            EndSchemaId = endMetaclass;
            Id = id;
            SchemaId = schemaId;
            Value = value;
            Version = version ?? DateTime.UtcNow.Ticks;
            NodeType = nodetype;
        }

        private MemoryGraphNode(MemoryGraphNode copy)
        {
            DebugContract.Requires(copy, "copy");

            Id = copy.Id;
            SchemaId = copy.SchemaId;
            _outgoings = copy._outgoings;
            _incomings = copy._incomings;
            Value = copy.Value;
            StartId = copy.StartId;
            StartSchemaId = copy.StartSchemaId;
            EndId = copy.EndId;
            EndSchemaId = copy.EndSchemaId;
            Version = DateTime.UtcNow.Ticks;
            NodeType = copy.NodeType;
        }

        private MemoryGraphNode(MemoryGraphNode copy, ImmutableDictionary<Identity, EdgeInfo> outgoings, ImmutableDictionary<Identity, EdgeInfo> incomings) : this(copy)
        {
            _outgoings = outgoings;
            _incomings = incomings;
        }

        private MemoryGraphNode(MemoryGraphNode copy, object value) : this(copy)
        {
            DebugContract.Requires(copy, "copy");
            Value = value;
        }

        /////-------------------------------------------------------------------------------------------------
        ///// <summary>
        /////  Default constructor.
        ///// </summary>
        /////-------------------------------------------------------------------------------------------------
        //public MemoryGraphNode()
        //{
        //}

        private IEnumerable<EdgeInfo> Incomings { get { return _incomings.Values; } }
        private IEnumerable<EdgeInfo> Outgoings { get { return _outgoings.Values; } }


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
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get; private set; }

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
        ///  Gets the start meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartSchemaId { get; private set; }

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
        ///  Gets the start.
        /// </summary>
        /// <value>
        ///  The identifier of the start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartId { get; private set; }

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
        ///  Sets a value.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <returns>
        ///  A MemoryGraphNode.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public MemoryGraphNode SetValue(object value)
        {
            return new MemoryGraphNode(this, value);
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
        public MemoryGraphNode AddEdge(Identity id, Identity metadataId, Direction direction, Identity endId, Identity endSchemaId)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(metadataId, "metadataId");
            DebugContract.Requires(endId, "endId");
            DebugContract.Requires(endSchemaId, "endSchemaId");

            if( (direction & Direction.Outgoing) == Direction.Outgoing && _outgoings.ContainsKey(id)
                || (direction & Direction.Incoming) == Direction.Incoming && _incomings.ContainsKey(id))            
                return this;

            var edge = new EdgeInfo(id, metadataId, endId, endSchemaId);

            return new MemoryGraphNode(this, 
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
        public MemoryGraphNode RemoveEdge(Identity id, Direction direction)
        {
            DebugContract.Requires(id, "id");

            if ((direction & Direction.Outgoing) == Direction.Outgoing && !_outgoings.ContainsKey(id)
                || (direction & Direction.Incoming) == Direction.Incoming && !_incomings.ContainsKey(id))
                return this;

            return new MemoryGraphNode(this, 
                    (direction & Direction.Outgoing) == Direction.Outgoing ? _outgoings.Remove(id) : _outgoings,  
                    (direction & Direction.Incoming) == Direction.Incoming ? _incomings.Remove(id) : _incomings
                );  
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the edges in this collection.
        /// </summary>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the edges in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<EdgeInfo> GetEdges(Direction direction)
        {
            return direction == Direction.Outgoing ? Outgoings : Incomings;
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