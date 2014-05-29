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
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.MemoryStore;
using Hyperstore.Modeling.Adapters;

#endregion

namespace Hyperstore.Modeling.HyperGraph.Adapters
{
    internal class CacheAdapter : IIndexManager, ICacheAdapter
    {
        private readonly IQueryGraphAdapter _adapter;
        private readonly MemoryGraphAdapter _memory;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <param name="adapter">
        ///  The adapter.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CacheAdapter(IDependencyResolver resolver, IQueryGraphAdapter adapter)
        {
            DebugContract.Requires(resolver);
            DebugContract.Requires(adapter);

            _memory = resolver.Resolve<MemoryGraphAdapter>() // Pour les tests
                      ?? new MemoryGraphAdapter(resolver);
            _adapter = adapter;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel { get; private set; }

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

            var node = _memory.GetGraphNode(id, metaclass);
            if (node == null 
                && (!localOnly && String.Compare(id.DomainModelName, DomainModel.Name, StringComparison.OrdinalIgnoreCase)==0)
                && _adapter != null 
                && (Session.Current == null || Session.Current.TrackingData.GetTrackingElementState(id) == TrackingState.Unknown)) // Il n'y a pas encore eu d'essai de chargement ds le cache
            {
                var result = _adapter.GetNode(id, metaclass);
                if (result != null)
                {
                    try
                    {
                        node = CreateMemoryNode(metaclass, result);
                    }
                    catch (DuplicateElementException)
                    {
                        // Cas ou il y a 2 création en //, on prend le risque d'une exception (qui ne peut arriver quand cas de conflit donc une seule fois)
                        // ce qui est moins pénalisant que de locker le noeud
                    }
                }
            }
            return node;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
        /// <param name="ownerId">
        ///  The identifier that owns this item.
        /// </param>
        /// <param name="ownerMetaclass">
        ///  The metaclass that owns this item.
        /// </param>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public PropertyValue GetPropertyValue(Identity ownerId, ISchemaElement ownerMetaclass, ISchemaProperty property)
        {
            DebugContract.Requires(ownerId);
            DebugContract.Requires(ownerMetaclass);
            DebugContract.Requires(property);

            // Lecture en cache
            // Vérification de l'existence des noeuds - Chargement ds le cache si nécessaire
            var start = GetGraphNode(ownerId, ownerMetaclass, false) as MemoryGraphNode;
            if (start == null)
                throw new InvalidElementException(ownerId);

            var prop = _memory.GetPropertyValue(ownerId, ownerMetaclass, property);
            // TODO : A voir 
            // NON pas de lazy sur le prop 
            //if (prop == null)
            //{
            //    // Si n'existe pas, on va devoir la lire dans l'adapter pour la stocker dans le cache
            //    Session.Current.AcquireLock(LockType.ExclusiveWait, ownerId);
            //    // On s'assure qu'il n'y est tjs pas
            //    prop = this._memory.GetPropertyValue(ownerId, ownerMetaclass, property);
            //    if (prop == null)
            //    {
            //        // Lecture dans l'adapteur
            //        prop = this._adapter.GetPropertyValue(ownerId, ownerMetaclass, property);
            //        if (prop != null)
            //            this._memory.SetPropertyValue(ownerId, ownerMetaclass, property, prop.Value, prop.Version);
            //    }

            //    // Si on n'a rien trouvé, on stocke en cache une valeur par défaut pour éviter d'essayer de la chercher
            //    // à chaque fois
            //    if (prop == null)
            //        this._memory.SetPropertyValue(ownerId, ownerMetaclass, property, property.DefaultValue, 0);
            //}

            return prop;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
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

            long? v = version;
            if (version == null)
            {
                var old = GetPropertyValue(owner.Id, owner.SchemaInfo, property); // TODO revoir signature (mel ?)
                if (old != null)
                    v = old.CurrentVersion;
            }
            else
            {
                var start = GetGraphNode(owner.Id, owner.SchemaInfo, false) as MemoryGraphNode;
                if (start == null)
                    throw new InvalidElementException(owner.Id);
                v = version.Value;
            }

            // La mise à jour dans l'adapteur se fera en fin de session
            var p = _memory.SetPropertyValue(owner, property, value, v);
            return p;
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
            var nodes = new HashSet<Identity>();
            foreach (var me in _memory.GetGraphNodes(elementType, metadata, true))
            {
                nodes.Add(me.Id);
                yield return me;
            }

            if (!localOnly && _adapter != null)
            {
                foreach (var edge in _adapter.GetNodes(elementType, metadata))
                {
                    if ((Session.Current == null || Session.Current.TrackingData.GetTrackingElementState(edge.Id) != TrackingState.Removed) && nodes.Add(edge.Id))
                    {
                        var m = edge.SchemaInfo;

                        var mem = _memory.GetGraphNode(edge.Id, m);
                        if (mem == null)
                            yield return CreateMemoryNode(m, edge);
                        else
                            yield return mem;
                    }
                }
            }
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

            // _adapter.CreateNode(id, metaClass);
            return _memory.CreateEntity(id, metaClass);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a relationship.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
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

            // Vérification de l'existence des noeuds - Chargement ds le cache si nécessaire
            var start = GetGraphNode(startId, startMetaclass, false) as MemoryGraphNode;
            if (start == null)
                throw new InvalidElementException(startId);

            // Si le noeud opposé se trouve dans un autre domaine, end sera null et le domaine cible ne sera pas
            // mis à jour. Seul le noeud source est impacté
            var end = GetGraphNode(endId, endMetaclass, false) as MemoryGraphNode;
            if (end == null && String.Compare(endId.DomainModelName, startId.DomainModelName, StringComparison.CurrentCultureIgnoreCase) == 0)
                throw new InvalidElementException(endId);

            return _memory.CreateRelationship(id, metaRelationship, startId, startMetaclass, endId, endMetaclass);
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

            _memory.RemoveEntity(node, metadata);
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

            _memory.RemoveRelationship(node, metadata);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the edges in this collection.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
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

            var edges = new HashSet<Identity>();
            foreach (var me in _memory.GetEdges(node, direction, metadata, true))
            {
                edges.Add(me.Id);
                yield return me;
            }

            if (!localOnly && _adapter != null && (Session.Current == null || Session.Current.TrackingData.GetTrackingElementState(node.Id) == TrackingState.Unknown))
            {
                foreach (var edge in _adapter.GetEdges(node.Id, direction, metadata, true))
                {
                    if ((Session.Current == null || Session.Current.TrackingData.GetTrackingElementState(edge.Id) != TrackingState.Removed) && edges.Add(edge.Id))
                    {
                        var m = metadata ?? edge.SchemaInfo;
                        if (m == null)
                            throw new InvalidElementException(edge.Id, "Inconsistant type");

                        var mem = _memory.GetGraphNode(edge.Id, m);
                        if (mem == null)
                            yield return CreateMemoryNode(m, edge);
                        else
                            yield return mem;
                    }
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a domain.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);

            DomainModel = domainModel;
            ((IDomainService) _memory).SetDomain(domainModel);
            _adapter.SetDomain(domainModel);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the index.
        /// </summary>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="unique">
        ///  true to unique.
        /// </param>
        /// <param name="propertyNames">
        ///  List of names of the properties.
        /// </param>
        /// <returns>
        ///  The new index.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IIndex CreateIndex(ISchemaElement metaclass, string name, bool unique, params string[] propertyNames)
        {
            DebugContract.Requires(metaclass);
            DebugContract.RequiresNotEmpty(name);
            IIndex index1 = null;

            var indexManager = _adapter as IIndexManager;
            if (indexManager != null)
                index1 = indexManager.CreateIndex(metaclass, name, unique, propertyNames);
            var index2 = _memory.IndexManager.CreateIndex(metaclass, name, unique, propertyNames);
            return index1 ?? index2;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Drops the index.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void DropIndex(string name)
        {
            DebugContract.RequiresNotEmpty(name);

            var indexManager = _adapter as IIndexManager;
            if (indexManager != null)
                indexManager.DropIndex(name);
            _memory.IndexManager.DropIndex(name);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Retourne un index.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The index.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IIndex GetIndex(string name)
        {
            DebugContract.RequiresNotEmpty(name);

            var indexManager = _adapter as IIndexManager;
            if (indexManager != null)
                return indexManager.GetIndex(name);

            return _memory.IndexManager.GetIndex(name);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (_adapter is IDisposable)
                ((IDisposable) _adapter).Dispose();
            _memory.Dispose();
        }

        private IGraphNode CreateMemoryNode(ISchemaInfo metaclass, QueryNodeResult node)
        {
            DebugContract.Requires(metaclass);
            DebugContract.Requires(node);

            MemoryGraphNode mem;
            if (node.NodeType == NodeType.Edge)
            {
                var sm = _memory.DomainModel.Store.GetSchemaEntity(node.StartSchemaId);
                var em = _memory.DomainModel.Store.GetSchemaEntity(node.EndSchemaId);
                mem = _memory.CreateRelationship(node.Id, (ISchemaRelationship) metaclass, node.StartId, sm, node.EndId, em) as MemoryGraphNode;
            }
            else
                mem = _memory.CreateEntity(node.Id, (ISchemaEntity) metaclass) as MemoryGraphNode;


            //if (this._adapter.SupportsLazyLoading == false)
            //{
            //    foreach (var edge in this._adapter.GetEdges(node, Direction.Outgoing))
            //    {
            //        mem.AddEdge(edge.Id, edge.MetaClassId, edge.End, edge.EndMetadata, Direction.Outgoing);
            //    }
            //    foreach (var edge in this._adapter.GetEdges(node, Direction.Incoming))
            //    {
            //        mem.AddEdge(edge.Id, edge.MetaClassId, edge.End, edge.EndMetadata, Direction.Incoming);
            //    }
            //}

            foreach (var prop in node.Properties)
            {
                _memory.SetProperty(mem.Id, metaclass as ISchemaElement, prop.Key, prop.Value.Value, prop.Value.CurrentVersion);
            }

            return mem;
        }
    }
}