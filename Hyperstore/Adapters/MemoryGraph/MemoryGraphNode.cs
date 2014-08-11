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

#endregion

namespace Hyperstore.Modeling.HyperGraph.Adapters
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
    public class MemoryGraphNode : IGraphNode, IDisposable, ICloneable<IGraphNode>
    {
        private string DebuggerDisplayString
        {
            get { return String.Format("Id={0}, Start={1}, End={2}, Value={3}", Id, StartId, EndId, Value); }
        }

        private readonly object _sync = new object();

        /// -------------------------------------------------------------------------------------------------
        ///  <summary>
        ///   Constructor.
        ///  </summary>
        ///  <param name="id">
        ///   The identifier.
        ///  </param>
        ///  <param name="metaClassId">
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
        public MemoryGraphNode(Identity id, Identity metaClassId, NodeType nodetype, Identity start = null, Identity startMetaclass = null, Identity end = null, Identity endMetaclass = null, object value = null, long version = 1)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(metaClassId, "metaClassId");
            DebugContract.Requires(start == null || startMetaclass != null, "start");
            DebugContract.Requires(end == null || endMetaclass != null, "end");
            DebugContract.Requires(start == null || end != null, "start/end");

            StartId = start;
            EndId = end;
            StartSchemaId = startMetaclass;
            EndSchemaId = endMetaclass;
            Id = id;
            SchemaId = metaClassId;
            Incomings = new EdgeList();
            Outgoings = new EdgeList();
            Value = value;
            Version = version;
            NodeType = nodetype;
        }

        internal MemoryGraphNode(MemoryGraphNode copy, long version)
        {
            DebugContract.Requires(copy, "copy");

            Id = copy.Id;
            SchemaId = copy.SchemaId;
            Incomings = copy.Incomings;
            Outgoings = copy.Outgoings;

            Value = copy.Value;
            StartId = copy.StartId;
            StartSchemaId = copy.StartSchemaId;
            EndId = copy.EndId;
            EndSchemaId = copy.EndSchemaId;
            Version = version;
            NodeType = copy.NodeType;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public MemoryGraphNode()
        {
        }

        private EdgeList Incomings { get; set; }
        private EdgeList Outgoings { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string DomainModel
        {
            get { return Id.DomainModelName; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Value { get; set; }

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

        IGraphNode ICloneable<IGraphNode>.Clone()
        {
            return new MemoryGraphNode(this, Version);
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
        public void AddEdge(Identity id, Identity metadataId, Direction direction, Identity endId, Identity endSchemaId)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(metadataId, "metadataId");
            DebugContract.Requires(endId, "endId");
            DebugContract.Requires(endSchemaId, "endSchemaId");

            var edge = new EdgeInfo(id, metadataId, endId, endSchemaId);
            lock (_sync)
            {
                if ((direction & Direction.Outgoing) == Direction.Outgoing)
                    Outgoings = Outgoings.Add(edge);
                else
                    Incomings = Incomings.Add(edge);
            }
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
        public void RemoveEdge(Identity id, Direction direction)
        {
            DebugContract.Requires(id, "id");
            lock (_sync)
            {
                if ((direction & Direction.Outgoing) == Direction.Outgoing)
                    Outgoings = Outgoings.RemoveByKey(id);
                if ((direction & Direction.Incoming) == Direction.Incoming)
                    Incomings = Incomings.RemoveByKey(id);
            }
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
            IEnumerable<EdgeInfo> list;
            lock (_sync)
            {
                list = direction == Direction.Outgoing ? Outgoings : Incomings;
            }

            return list;
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