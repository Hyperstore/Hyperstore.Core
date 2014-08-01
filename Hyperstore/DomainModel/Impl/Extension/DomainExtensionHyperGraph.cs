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
#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal class DomainExtensionHyperGraph : HyperGraph.HyperGraph, IExtensionHyperGraph
    {
        private readonly IDomainModel _extendedDomain;
        private readonly ExtendedMode _extensionMode;

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
        public DomainExtensionHyperGraph(IDependencyResolver resolver, IDomainModel extendedDomain, ExtendedMode mode) : base(resolver)
        {
            DebugContract.Requires(resolver);
            DebugContract.Requires(extendedDomain);

            _extendedDomain = extendedDomain;
            _extensionMode = mode;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve graph adapter.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <returns>
        ///  An ICacheAdapter.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override ICacheAdapter ResolveGraphAdapter()
        {
            var adapter = base.ResolveGraphAdapter();
            if (adapter == null)
                throw new Exception(ExceptionMessages.CantExtendDomainModel_GraphAdapter);

            var innerGraph = _extendedDomain.Resolve<IHyperGraph>();
            var extendedDomainModelAdapter = innerGraph.Adapter;
            if (extendedDomainModelAdapter == null)
                throw new Exception(ExceptionMessages.CantExtendDomainModel_GraphAdapter);

            return new DomainExtensionAdapter(adapter, extendedDomainModelAdapter, _extensionMode);
        }

        System.Collections.Generic.IEnumerable<IModelElement> IExtensionHyperGraph.GetExtensionElements(ISchemaElement schemaElement)
        {
            var adapter = Adapter as IExtensionAdapter;
            var query = adapter.GetExtensionGraphNodes(NodeType.EdgeOrNode, schemaElement);
            return base.GetElementsCore<IModelElement>(query, schemaElement, 0, true);
        }

        System.Collections.Generic.IEnumerable<Tuple<Identity,Identity>> IExtensionHyperGraph.GetDeletedElements()
        {
            var adapter = Adapter as IExtensionAdapter;
            return adapter.GetDeletedGraphNodes().Select( n => Tuple.Create(n.Id, n.SchemaId));
        }

        System.Collections.Generic.IEnumerable<IModelRelationship> IExtensionHyperGraph.GetExtensionRelationships(ISchemaRelationship schemaRelationship, IModelElement start, IModelElement end)
        {
            var adapter = Adapter as IExtensionAdapter;
            IEnumerable<IGraphNode> query;
            if (start != null)
            {
                var node = adapter.GetGraphNode(start.Id, start.SchemaInfo, true);
                if (node != null)
                {
                        query = adapter.GetExtensionEdges(node, Direction.Outgoing, schemaRelationship);
                        if (end != null)
                            query = query.Where(n => n.EndId == end.Id);
                        return GetRelationshipsCore<IModelRelationship>(query, 0, schemaRelationship);
                }
                return Enumerable.Empty<IModelRelationship>();
            }
            else if (end != null)
            {
                var node = adapter.GetGraphNode(end.Id, end.SchemaInfo, true);
                if (node != null)
                {
                    query = adapter.GetExtensionEdges(node, Direction.Incoming, schemaRelationship);
                    return GetRelationshipsCore<IModelRelationship>(query, 0, schemaRelationship);
                }
                return Enumerable.Empty<IModelRelationship>();
            }

            query = adapter.GetExtensionGraphNodes(NodeType.Edge, schemaRelationship);
            return GetRelationshipsCore<IModelRelationship>(query, 0, schemaRelationship);

        }

        System.Collections.Generic.IEnumerable<IModelEntity> IExtensionHyperGraph.GetExtensionEntities(ISchemaEntity schemaEntity)
        {
            var adapter = Adapter as IExtensionAdapter;
            var query = adapter.GetExtensionGraphNodes(NodeType.Node, schemaEntity);
            return base.GetElementsCore<IModelEntity>(query, schemaEntity, 0, true);
        }
    }
}