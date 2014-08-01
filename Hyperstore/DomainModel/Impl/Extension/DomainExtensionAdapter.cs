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
using System.Linq;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Adapters;

#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal class DomainExtensionAdapter : ICacheAdapter, IDisposable, IExtensionAdapter
    {
        private readonly ICacheAdapter _extendedDomainAdapter;
        private readonly ICacheAdapter _extensionAdapter;
        private readonly ExtendedMode _extensionMode;

        private IKeyValueStore _deletedElements;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModelAdapter">
        ///  The domain model adapter.
        /// </param>
        /// <param name="extendedDomainModelAdapter">
        ///  The extended domain model adapter.
        /// </param>
        /// <param name="extensionMode">
        ///  The extension mode.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainExtensionAdapter(ICacheAdapter domainModelAdapter, ICacheAdapter extendedDomainModelAdapter, ExtendedMode extensionMode)
        {
            DebugContract.Requires(extendedDomainModelAdapter, "extendedDomainModelAdapter");
            DebugContract.Requires(domainModelAdapter, "domainModelAdapter");

            _extendedDomainAdapter = extendedDomainModelAdapter;
            _extensionAdapter = domainModelAdapter;
            _extensionMode = extensionMode;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Correspond à l'extension de domaine.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a domain.
        /// </summary>
        /// <param name="domainModel">
        ///  Correspond à l'extension de domaine.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel, "domainModel");
            DomainModel = domainModel;
            _extensionAdapter.SetDomain(domainModel);
            _deletedElements = new Hyperstore.Modeling.MemoryStore.TransactionalMemoryStore(
                                        _extensionAdapter.DomainModel.Name,
                                        5,
                                        _extendedDomainAdapter.DomainModel.DependencyResolver.Resolve<Hyperstore.Modeling.MemoryStore.ITransactionManager>()
                                        );
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets graph node.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <param name="localOnly">
        ///  true to local only.
        /// </param>
        /// <returns>
        ///  The graph node.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IGraphNode GetGraphNode(Identity id, ISchemaElement metaclass, bool localOnly)
        {
            DebugContract.Requires(id);
            DebugContract.Requires(metaclass);

            if (_deletedElements.GetNode(id, null) != null)
                return null;

            var node = _extensionAdapter.GetGraphNode(id, metaclass, localOnly);
            if (node != null)
                return node;

            return _extendedDomainAdapter.GetGraphNode(id, metaclass, localOnly);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the graph nodes in this collection.
        /// </summary>
        /// <param name="elementType">
        ///  Type of the element.
        /// </param>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="localOnly">
        ///  true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the graph nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IGraphNode> GetGraphNodes(NodeType elementType, ISchemaElement metadata, bool localOnly)
        {
            var identities = new HashSet<Identity>();
            foreach (var node in _extensionAdapter.GetGraphNodes(elementType, metadata, localOnly))
            {
                identities.Add(node.Id);
                if (_deletedElements.GetNode(node.Id, null) == null)
                    yield return node;
            }

            foreach (var node in _extendedDomainAdapter.GetGraphNodes(elementType, metadata, localOnly))
            {
                if (identities.Add(node.Id))
                    yield return node;
            }
        }

        public IEnumerable<IGraphNode> GetExtensionGraphNodes(NodeType elementType, ISchemaElement metadata)
        {
            foreach (var node in _extensionAdapter.GetGraphNodes(elementType, metadata, true))
            {
                if (_deletedElements.GetNode(node.Id, null) == null)
                    yield return node;
            }
        }

        public IEnumerable<IGraphNode> GetExtensionEdges(IGraphNode node, Direction direction, ISchemaRelationship schemaRelationship)
        {
            DebugContract.Requires(node);

            foreach (var edge in _extensionAdapter.GetEdges(node, direction, schemaRelationship, true))
            {
                if (edge != null)
                {
                    if (_deletedElements.GetNode(edge.Id, null) == null)
                        yield return edge;
                }
            }
        }

        public IEnumerable<IGraphNode> GetDeletedGraphNodes()
        {
            return _deletedElements.GetAllNodes(NodeType.EdgeOrNode);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates an entity.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaClass">
        ///  The meta class.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IGraphNode CreateEntity(Identity id, ISchemaEntity metaClass)
        {
            DebugContract.Requires(id);
            DebugContract.Requires(metaClass);

            _deletedElements.RemoveNode(id);
            if (IsInMode(ExtendedMode.ReadOnly))
                return _extensionAdapter.CreateEntity(id, metaClass);

            return _extendedDomainAdapter.CreateEntity(id, metaClass);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaRelationship">
        ///  The meta relationship.
        /// </param>
        /// <param name="startId">
        ///  The start identifier.
        /// </param>
        /// <param name="startMetaclass">
        ///  The start metaclass.
        /// </param>
        /// <param name="endId">
        ///  The end identifier.
        /// </param>
        /// <param name="endMetaclass">
        ///  The end metaclass.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IGraphNode CreateRelationship(Identity id, ISchemaRelationship metaRelationship, Identity startId, ISchemaElement startMetaclass, Identity endId, ISchemaElement endMetaclass)
        {
            DebugContract.Requires(id);
            DebugContract.Requires(metaRelationship);
            DebugContract.Requires(startId);
            DebugContract.Requires(startMetaclass);
            DebugContract.Requires(endId);
            DebugContract.Requires(endMetaclass);

            _deletedElements.RemoveNode(id);
            if (IsInMode(ExtendedMode.ReadOnly))
                return _extensionAdapter.CreateRelationship(id, metaRelationship, startId, startMetaclass, endId, endMetaclass);

            return _extendedDomainAdapter.CreateRelationship(id, metaRelationship, startId, startMetaclass, endId, endMetaclass);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the entity.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RemoveEntity(IGraphNode node, ISchemaEntity metadata)
        {
            DebugContract.Requires(node);
            DebugContract.Requires(metadata);

            _deletedElements.AddNode(node, null);
            _extensionAdapter.RemoveEntity(node, metadata);

            if (IsInMode(ExtendedMode.ReadOnly))
                return;

            _extendedDomainAdapter.RemoveEntity(node, metadata);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RemoveRelationship(IGraphNode node, ISchemaRelationship metadata)
        {
            DebugContract.Requires(node);
            DebugContract.Requires(metadata);

            _deletedElements.AddNode(node, null);
            _extensionAdapter.RemoveRelationship(node, metadata);

            if (IsInMode(ExtendedMode.ReadOnly))
            {
                return;
            }

            _extendedDomainAdapter.RemoveRelationship(node, metadata);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the edges in this collection.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="localOnly">
        ///  true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the edges in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IGraphNode> GetEdges(IGraphNode node, Direction direction, ISchemaRelationship metadata, bool localOnly)
        {
            DebugContract.Requires(node);

            var identities = new HashSet<Identity>();
            foreach (var edge in _extensionAdapter.GetEdges(node, direction, metadata, localOnly))
            {
                if (edge != null)
                {
                    identities.Add(edge.Id);
                    if (_deletedElements.GetNode(edge.Id, null) == null)
                        yield return edge;
                }
            }

            foreach (var edge in _extendedDomainAdapter.GetEdges(node, direction, metadata, localOnly)
                    .Where(edge => identities.Add(edge.Id)))
            {
                yield return edge;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <param name="ownerId">
        ///  The identifier that owns this item.
        /// </param>
        /// <param name="ownerMetadata">
        ///  The metadata that owns this item.
        /// </param>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public PropertyValue GetPropertyValue(Identity ownerId, ISchemaElement ownerMetadata, ISchemaProperty property)
        {
            DebugContract.Requires(ownerId);
            DebugContract.Requires(ownerMetadata);
            DebugContract.Requires(property);

            if (_deletedElements.GetNode(ownerId, null) != null)
                throw new InvalidElementException(ownerId);

            // on recherche dans l'extension
            PropertyValue p = _extensionAdapter.GetPropertyValue(ownerId, ownerMetadata, property);
            if (p != null) //|| IsInMode(ExtendedMode.ReadOnly) */|| IsDefinedOnlyInExtension(property))// obligatoirement faux ici || IsDefinedInExtension(ownerMetadata))
                return p;

            // Sinon, on le cherche dans le domaine étendu.
            return _extendedDomainAdapter.GetPropertyValue(ownerId, ownerMetadata, property);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value.
        /// </summary>
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        /// <returns>
        ///  A PropertyValue.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public PropertyValue SetPropertyValue(IModelElement owner, ISchemaProperty property, object value, long? version)
        {
            DebugContract.Requires(owner);
            DebugContract.Requires(property);

            if (_deletedElements.GetNode(owner.Id, null) != null)
                throw new InvalidElementException(owner.Id);

            // Mise à jour dans le domaine d'extension d'une propriété faisant partie du domaine étendu
            if (//IsDefinedOnlyInExtension(owner.SchemaInfo) ||  // Le owner n'est pas un élément du domaine étendu (directement)
                //IsDefinedOnlyInExtension(property) ||          // La propriété a été définie dans l'extension
                IsInMode(ExtendedMode.ReadOnly)                // On est en mode surcharge, on ne touche pas au domaine éténdu
               )
            {
                // Il est peut-être nécéssaire de créer le noeud parent dans le domaine d'extension
                if (_extensionAdapter.GetGraphNode(owner.Id, owner.SchemaInfo, false) == null)
                {
                    _deletedElements.RemoveNode(owner.Id);

                    var rel = owner as IModelRelationship;
                    if (rel == null)
                        _extensionAdapter.CreateEntity(owner.Id, (ISchemaEntity)owner.SchemaInfo);
                    else
                        _extensionAdapter.CreateRelationship(rel.Id, (ISchemaRelationship)rel.SchemaInfo, rel.Start.Id, rel.Start.SchemaInfo, rel.End.Id, rel.End.SchemaInfo);
                }

                var pnode = _extensionAdapter.SetPropertyValue(owner, property, value, version);
                // Si la valeur n'existait pas dans l'extension, on regarde si elle n'existait pas le domaine étendu
                if (pnode == null)
                {
                    pnode = _extendedDomainAdapter.GetPropertyValue(owner.Id, owner.SchemaInfo, property);
                }
                return pnode;
            }

            // Mise à jour de la propriété directement dans le domaine étendu
            return _extendedDomainAdapter.SetPropertyValue(owner, property, value, version);
        }

        //private bool IsElementDefinedInExtension(ISchemaInfo metaclass)
        //{
        //    DebugContract.Requires(metaclass);

        //    var sp = metaclass;
        //    while (sp != null && !sp.IsPrimitive)
        //    {
        //        if (sp.DomainModel.InstanceId == _extendedDomainAdapter.DomainModel.Schema.InstanceId)
        //            return false;
        //        sp = sp.SuperClass;
        //    }
        //    return true;
        //}

        private bool IsInMode(ExtendedMode mode)
        {
            return (_extensionMode & mode) == mode;
        }

        //    private bool IsDefinedOnlyInExtension(ISchemaInfo schema)
        //    {
        //        DebugContract.Requires(schema);

        //        if (schema is ISchemaProperty)
        //        {
        //            return schema.DomainModel.InstanceId == DomainModel.Schema.InstanceId;
        //        }

        //        var sp = schema;
        //        while (sp != null && !sp.IsPrimitive)
        //        {
        //            if (sp.DomainModel.InstanceId == _extendedDomainAdapter.DomainModel.Schema.InstanceId)
        //                return false;
        //            sp = sp.SuperClass;
        //        }
        //        return true;
        //    }

        void IDisposable.Dispose()
        {
            var disposable = _deletedElements as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            disposable = _extensionAdapter as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}