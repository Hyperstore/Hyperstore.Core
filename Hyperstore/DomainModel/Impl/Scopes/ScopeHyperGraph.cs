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
using Hyperstore.Modeling.HyperGraph;
using System.Collections.Generic;
using System.Linq;
using Hyperstore.Modeling.Domain;
#endregion

namespace Hyperstore.Modeling.Scopes
{
    internal class ScopeHyperGraph : HyperGraph.HyperGraph, IScopeHyperGraph
    {
        private readonly IDomainModel _extendedDomain;
        private readonly HyperGraph.HyperGraph _extendedGraph;
        private IKeyValueStore _deletedElements;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="services">
        ///  The services.
        /// </param>
        /// <param name="extendedDomain">
        ///  The extended domain.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ScopeHyperGraph(IServicesContainer services, IHyperGraphProvider extendedDomain) : base(services)
        {
            DebugContract.Requires(services);
            DebugContract.Requires(extendedDomain);

            _extendedDomain = extendedDomain;

            _extendedGraph = extendedDomain.InnerGraph as HyperGraph.HyperGraph;
            System.Diagnostics.Debug.Assert(_extendedGraph != null);
            _deletedElements = new Hyperstore.Modeling.MemoryStore.TransactionalMemoryStore(
                            _extendedDomain.Name,
                            5,
                            _extendedDomain.Services.Resolve<Hyperstore.Modeling.MemoryStore.ITransactionManager>()
                            );
        }

        public override bool IsDeleted(Identity id)
        {
            return _deletedElements.Exists(id);
        }

        internal override bool GraphNodeExists(Identity id, ISchemaElement schemaElement)
        {
            return !IsDeleted(id) && (base.GraphNodeExists(id, schemaElement) || _extendedGraph.GraphNodeExists(id, schemaElement));
        }

        internal override bool GetGraphNode(Identity id, NodeType nodeType, ISchemaInfo schemaInfo, out GraphNode node)
        {
            node = null;
            if (IsDeleted(id))
                return false;

            if (base.GetGraphNode(id, nodeType, schemaInfo, out node) && node != null)
                return true;

            return _extendedGraph.GetGraphNode(id, nodeType, schemaInfo, out node);
        }

        internal override IEnumerable<GraphNode> GetGraphNodes(NodeType nodetype)
        {
            HashSet<Identity> set = new HashSet<Identity>();
            foreach(var node in base.GetGraphNodes(nodetype))
            {
                set.Add(node.Id);
                if (IsDeleted(node.Id))
                    continue;
                yield return node;
            }

            foreach (var node in _extendedGraph.GetGraphNodes(nodetype))
            {
                if( !set.Add(node.Id) || IsDeleted(node.Id))
                    continue;
                yield return node;
            }
        }

        internal override IEnumerable<EdgeInfo> GetGraphEdges(GraphNode source, ISchemaElement sourceSchema, Direction direction)
        {
            HashSet<Identity> set = new HashSet<Identity>();
            GraphNode node = source;
            if (node != null)
            {
                foreach (var edge in base.GetGraphEdges(node, sourceSchema, direction))
                {
                    set.Add(edge.Id);
                    if (!IsDeleted(edge.Id))
                        yield return edge;
                }
            }

            if (_extendedGraph.GetGraphNode(source.Id, NodeType.Edge, sourceSchema, out node) && node != null)
            {
                foreach (var edge in _extendedGraph.GetGraphEdges(node, sourceSchema, direction))
                {
                    if (set.Add(edge.Id) && !IsDeleted(edge.Id))
                        yield return edge;
                }
            }
        }

        protected override Tuple<GraphNode, GraphNode> GetTerminalNodes(Identity startId, ISchemaInfo startSchema, Identity endId, ISchemaInfo endSchema)
        {
            GraphNode start = null;
            GraphNode end = null;

            if (!IsDeleted(startId))
            {
                base.GetGraphNode(startId, NodeType.EdgeOrNode, startSchema, out start);
                if (start == null)
                {
                    GraphNode extendedStart;
                    _extendedGraph.GetGraphNode(startId, NodeType.EdgeOrNode, startSchema, out extendedStart);
                    if (extendedStart != null)
                    {
                        var rel = extendedStart as IModelRelationship;
                        if (rel == null)
                            start = CreateEntity(startId, (ISchemaEntity)startSchema);
                        else
                            start = CreateRelationship(startId, (ISchemaRelationship)startSchema, rel.Start.Id, rel.Start.SchemaInfo, rel.End.Id, rel.End.SchemaInfo);
                    }
                }
            }

            if (startId.DomainModelName == endId.DomainModelName && !IsDeleted(endId))
            {
                base.GetGraphNode(endId, NodeType.EdgeOrNode, endSchema, out end);
                if (end == null)
                {
                    GraphNode extendedEnd;
                    _extendedGraph.GetGraphNode(endId, NodeType.EdgeOrNode, endSchema, out extendedEnd);
                    if (extendedEnd != null)
                    {
                        var rel = extendedEnd as IModelRelationship;
                        if (rel == null)
                            end = CreateEntity(endId, (ISchemaEntity)endSchema);
                        else
                            end = CreateRelationship(endId, (ISchemaRelationship)endSchema, rel.Start.Id, rel.Start.SchemaInfo, rel.End.Id, rel.End.SchemaInfo);
                    }
                }
            }

            return Tuple.Create(start as GraphNode, end as GraphNode);
        }

        public override GraphNode CreateEntity(Identity id, ISchemaEntity schemaEntity)
        {
            var flag = base.CreateEntity(id, schemaEntity);
            _deletedElements.RemoveNode(id);
            return flag;
        }

        public override bool RemoveEntity(Identity id, ISchemaEntity schemaEntity, bool throwExceptionIfNotExists)
        {
            var flag = base.RemoveEntity(id, schemaEntity, false);
            _deletedElements.AddNode(new GraphNode(id, schemaEntity.Id, NodeType.Node));
            return flag;
        }


        public override GraphNode CreateRelationship(Identity id, ISchemaRelationship metaRelationship, Identity startId, ISchemaElement startSchema, Identity endId, ISchemaElement endSchema)
        {
            var flag = base.CreateRelationship(id, metaRelationship, startId, startSchema, endId, endSchema);
            _deletedElements.RemoveNode(id);
            return flag;
        }

        public override bool RemoveRelationship(Identity id, ISchemaRelationship schemaRelationship, bool throwExceptionIfNotExists)
        {
            var flag = base.RemoveRelationship(id, schemaRelationship, false);
            _deletedElements.AddNode(new GraphNode(id, schemaRelationship.Id, NodeType.Node));
            return flag;
        }

        IEnumerable<IModelElement> IScopeHyperGraph.GetExtensionElements(ISchemaElement schemaElement)
        {
            var query = base.GetGraphNodes(NodeType.EdgeOrNode);
            return base.GetElementsCore<IModelElement>(query, schemaElement, 0);
        }

        System.Collections.Generic.IEnumerable<GraphNode> IScopeHyperGraph.GetDeletedElements()
        {
            return _deletedElements.GetAllNodes(NodeType.EdgeOrNode);
        }

        public override PropertyValue SetPropertyValue(IModelElement owner, ISchemaProperty property, object value, long? version)
        {
            if (!GraphNodeExists(owner.Id, owner.SchemaInfo))
                throw new InvalidElementException(owner.Id);

            var pid = owner.Id.CreateAttributeIdentity(property.Name);
            GraphNode propertyNode;
            _extendedGraph.GetGraphNode(pid, NodeType.Property, property, out propertyNode); // Potential old value

            if (!base.GraphNodeExists(owner.Id, owner.SchemaInfo))
            {
                var rel = owner as IModelRelationship;
                if (rel == null)
                    CreateEntity(owner.Id, (ISchemaEntity)owner.SchemaInfo);
                else
                    CreateRelationship(rel.Id, (ISchemaRelationship)rel.SchemaInfo, rel.Start.Id, rel.Start.SchemaInfo, rel.End.Id, rel.End.SchemaInfo);
            }

            return base.SetPropertyValueCore(owner, property, value, version, propertyNode);
        }
    }
}