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
using Hyperstore.Modeling.HyperGraph;
using System.Collections.Generic;
using System.Linq;
using Hyperstore.Modeling.HyperGraph.Adapters;
using Hyperstore.Modeling.Domain;
#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal class DomainExtensionHyperGraph : HyperGraph.HyperGraph, IExtensionHyperGraph
    {
        private readonly IDomainModel _extendedDomain;
        private readonly ExtendedMode _extensionMode;
        private readonly HyperGraph.HyperGraph _extendedGraph;
        private IKeyValueStore _deletedElements;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <param name="extendedDomain">
        ///  The extended domain.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainExtensionHyperGraph(IDependencyResolver resolver, IHyperGraphProvider extendedDomain, ExtendedMode mode) : base(resolver)
        {
            DebugContract.Requires(resolver);
            DebugContract.Requires(extendedDomain);

            _extendedDomain = extendedDomain;
            _extensionMode = mode;

            _extendedGraph = extendedDomain.InnerGraph as HyperGraph.HyperGraph;
            System.Diagnostics.Debug.Assert(_extendedGraph != null);
            _deletedElements = new Hyperstore.Modeling.MemoryStore.TransactionalMemoryStore(
                            _extendedDomain.Name,
                            5,
                            _extendedDomain.DependencyResolver.Resolve<Hyperstore.Modeling.MemoryStore.ITransactionManager>()
                            );
        }

        private bool IsInMode(ExtendedMode mode)
        {
            return (_extensionMode & mode) == mode;
        }

        public override bool IsDeleted(Identity id)
        {
            return _deletedElements.Exists(id);
        }

        internal override bool GraphExists(Identity id)
        {
            return !IsDeleted(id) && (base.GraphExists(id) || _extendedGraph.GraphExists(id));
        }

        internal override bool GetGraphNode(Identity id, out IGraphNode node)
        {
            node = null;
            if (IsDeleted(id))
                return false;

            if (base.GetGraphNode(id, out node) && node != null)
                return true;

            return _extendedGraph.GetGraphNode(id, out node);
        }

        internal override IEnumerable<IGraphNode> GetGraphNodes(NodeType nodetype)
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

        public override IGraphNode CreateEntity(Identity id, ISchemaEntity schemaEntity)
        {
            _deletedElements.RemoveNode(id);
//            if (IsInMode(ExtendedMode.ReadOnly))
                return base.CreateEntity(id, schemaEntity);
//            return _extendedGraph.CreateEntity(id, schemaEntity);
        }

        public override bool RemoveEntity(Identity id, ISchemaEntity schemaEntity, bool throwExceptionIfNotExists)
        {
            _deletedElements.AddNode(new MemoryGraphNode(id, schemaEntity.Id, NodeType.Node));

            var flag = base.RemoveEntity(id, schemaEntity, false);

            if (IsInMode(ExtendedMode.ReadOnly))
                return flag;

            return _extendedGraph.RemoveEntity(id, schemaEntity, throwExceptionIfNotExists);
        }


        public override IGraphNode CreateRelationship(Identity id, ISchemaRelationship metaRelationship, Identity startId, ISchemaElement startSchema, Identity endId, ISchemaElement endSchema)
        {
            _deletedElements.RemoveNode(id);
//            if (IsInMode(ExtendedMode.ReadOnly))
                return base.CreateRelationship(id, metaRelationship, startId, startSchema, endId, endSchema);
//            return _extendedGraph.CreateRelationship(id, metaRelationship, startId, startSchema, endId, endSchema);
        }

        public override bool RemoveRelationship(Identity id, ISchemaRelationship schemaRelationship, bool throwExceptionIfNotExists)
        {
            _deletedElements.AddNode(new MemoryGraphNode(id, schemaRelationship.Id, NodeType.Node));
            var flag = base.RemoveRelationship(id, schemaRelationship, throwExceptionIfNotExists);
            if (IsInMode(ExtendedMode.ReadOnly))
                return flag; 
            return _extendedGraph.RemoveRelationship(id, schemaRelationship, throwExceptionIfNotExists);
        }

        IEnumerable<IModelElement> IExtensionHyperGraph.GetExtensionElements(ISchemaElement schemaElement)
        {
            var query = base.GetGraphNodes(NodeType.EdgeOrNode);
            return base.GetElementsCore<IModelElement>(query, schemaElement, 0);
        }

        System.Collections.Generic.IEnumerable<INodeInfo> IExtensionHyperGraph.GetDeletedElements()
        {
            return _deletedElements.GetAllNodes(NodeType.EdgeOrNode);
        }

        public override PropertyValue SetPropertyValue(IModelElement owner, ISchemaProperty property, object value, long? version)
        {
            if (_deletedElements.GetNode(owner.Id) != null)
                throw new InvalidElementException(owner.Id);

            var pid = owner.Id.CreateAttributeIdentity(property.Name);

            // If we are in Read only mode OR if the property node doesn't exist in the extended domain then the
            // property will be set in the extension.
            IGraphNode propertyNode;
            // Don't change the sequence order of the following expression
            if (!_extendedGraph.GetGraphNode(pid, out propertyNode) || IsInMode(ExtendedMode.ReadOnly))
            {
                IGraphNode ownerNode;
                if (!base.GetGraphNode(owner.Id, out ownerNode))
                {
                    var rel = owner as IModelRelationship;
                    if (rel == null)
                        base.CreateEntity(owner.Id, (ISchemaEntity)owner.SchemaInfo);
                    else
                        base.CreateRelationship(rel.Id, (ISchemaRelationship)rel.SchemaInfo, rel.Start.Id, rel.Start.SchemaInfo, rel.End.Id, rel.End.SchemaInfo);
                }

                return base.SetPropertyValueCore(owner, property, value, version, propertyNode);
            }

            return _extendedGraph.SetPropertyValue(owner, property, value, version);
        }
    }
}