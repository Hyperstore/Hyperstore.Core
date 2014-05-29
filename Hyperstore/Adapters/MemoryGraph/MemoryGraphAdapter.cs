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
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Modeling.MemoryStore;
using Hyperstore.Modeling.Adapters;

#endregion

namespace Hyperstore.Modeling.HyperGraph.Adapters
{
    internal class MemoryGraphAdapter : ICacheAdapter, IIndexManager
    {
        private const string CONTEXT_KEY = "__MGA__";

        private readonly IDependencyResolver _resolver;
        private bool _disposed;
        private IHyperstore _store;

        internal MemoryGraphAdapter()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public MemoryGraphAdapter(IDependencyResolver resolver)
        {
            Contract.Requires(resolver, "resolver");

            _resolver = resolver;
        }

        internal IKeyValueStore Storage { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the manager for index.
        /// </summary>
        /// <value>
        ///  The index manager.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public MemoryIndexManager IndexManager { get; private set; }
        internal virtual HypergraphTransaction CurrentTransaction
        {
            get
            {
                var session = Session.Current;
                if (session == null)
                    throw new NotInTransactionException();

                var ctx = session.GetContextInfo<HypergraphTransaction>(CONTEXT_KEY);
                return ctx;
            }
            set
            {
                var session = Session.Current;
                if (session == null)
                    throw new NotInTransactionException();

                session.SetContextInfo(CONTEXT_KEY, value);
            }
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

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel, "domainModel");

            DomainModel = domainModel;
            var kv =_resolver.Resolve<IKeyValueStore>() ?? new TransactionalMemoryStore(domainModel);
            Storage = kv;
            if (kv is IDomainService)
                ((IDomainService)kv).SetDomain(domainModel);

            IndexManager = new MemoryIndexManager(this);
            _store = _resolver.Resolve<IHyperstore>();
            var evictionPolicy = Storage.EvictionPolicy;
            if (evictionPolicy != null)
                evictionPolicy.ElementEvicted += OnElementEvicted;
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
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The graph node.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IGraphNode GetGraphNode(Identity id, ISchemaElement metaclass, bool localOnly = true)
        {
            DebugContract.Requires(id);
            return Storage.GetNode(id, metaclass);
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
            foreach (var node in Storage.GetAllNodes(elementType))
            {
                if (metadata == null || node.SchemaId == metadata.Id)
                    yield return node;
                else
                {
                    var nodeMetaClass = _store.GetSchemaInfo(node.SchemaId);
                    if (nodeMetaClass.IsA(metadata))
                        yield return node;
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

            if (Session.Current == null)
                throw new NotInTransactionException();

            using (var tx = BeginTransaction())
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction.UpdateProfiler(p => p.NodesCreated.Incr());
                    CurrentTransaction.UpdateProfiler(p => p.NumberOfNodes.Incr());
                }

                var node = new MemoryGraphNode(this.DomainModel.Store, id, metaClass.Id, NodeType.Node);
                Storage.AddNode(node, metaClass);

                tx.Commit();
                return node;
            }
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
            DebugContract.Requires(Session.Current);

            using (var tx = BeginTransaction())
            {
                if (Storage.RemoveNode(node.Id))
                {
                    foreach (var prop in metadata.GetProperties(true))
                    {
                        Storage.RemoveNode(node.Id.CreateAttributeIdentity(prop.Name));
                    }

                    // Informe qu'il faudra mettre à jour les index
                    DeferRemoveIndex(metadata, node.Id);

                    CurrentTransaction.UpdateProfiler(p => p.NodesDeleted.Incr());
                    CurrentTransaction.UpdateProfiler(p => p.NumberOfNodes.Dec());
                }
                tx.Commit();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
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
            DebugContract.Requires(ownerId);
            DebugContract.Requires(property);

            //var node = GetGraphNode(ownerId, ownerMetaclass) as MemoryGraphNode;
            //// Vérification si le owner existe
            //if (node == null)
            //    throw new InvalidElementException(ownerId);

            var pid = ownerId.CreateAttributeIdentity(property.Name);
            var p = Storage.GetNode(pid, property) as MemoryGraphNode;
            if (p == null)
                return null;

            return new PropertyValue
                   {
                           Value = p.Value,
                           CurrentVersion = p.Version
                   };

            // return node.GetProperty(DomainModel, property);
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
            DebugContract.Requires(Session.Current);

            return SetProperty(owner.Id, owner.SchemaInfo, property, value, version);
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

            if (Session.Current == null)
                throw new NotInTransactionException();

            using (var tx = BeginTransaction())
            {
                var node = new MemoryGraphNode(this.DomainModel.Store, id, metaRelationship.Id, NodeType.Edge, startId, startMetaclass.Id, endId, endMetaclass.Id);
                Storage.AddNode(node, metaRelationship);

                var start = GetGraphNode(startId, startMetaclass) as MemoryGraphNode;
                if (start == null)
                    throw new InvalidElementException(startId);

                // Si le noeud opposé se trouve dans un autre domaine, end sera null et le domaine cible ne sera pas
                // mis à jour. Seul le noeud source est impacté
                var end = GetGraphNode(endId, endMetaclass) as MemoryGraphNode;

                // Mise à jour des infos sur les relations propres à un noeud
                if (startId == endId)
                {
                    start.AddEdge(id, metaRelationship.Id, Direction.Both);
                    UpdateNode(start, startMetaclass);
                }
                else
                {
                    start.AddEdge(id, metaRelationship.Id, Direction.Outgoing);
                    UpdateNode(start, startMetaclass);

                    // Relation uni-directionnelle entre domaine.
                    if (end != null)
                    {
                        end.AddEdge(id, metaRelationship.Id,  Direction.Incoming);
                        UpdateNode(end, endMetaclass);
                    }
                }

                DeferAddIndex(metaRelationship, id);
                if (CurrentTransaction != null)
                {
                    // TODO arrive quand on met des données en cache qui ont été lues via un autre adapteur du coup
                    // si cela arrive ds une transaction la stat est fausse car ce n'est pas une véritable cr
                    CurrentTransaction.UpdateProfiler(p => p.RelationshipsCreated.Incr());
                    CurrentTransaction.UpdateProfiler(p => p.NumberOfEdges.Incr());
                }

                tx.Commit();
                return node;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
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
            DebugContract.Requires(Session.Current);

            using (var tx = BeginTransaction())
            {
                if (Storage.RemoveNode(node.Id))
                {
                    var startMetadata = _store.GetSchemaEntity(node.StartSchemaId);
                    var start = GetGraphNode(node.StartId, startMetadata) as MemoryGraphNode;
                    if (start == null)
                        throw new InvalidElementException(node.StartId);

                    // Si le noeud opposé se trouve dans un autre domaine, on ne le met pas à jour
                    // Seul le noeud source est impacté
                    MemoryGraphNode end = null;
                    if (node.StartId.DomainModelName == node.EndId.DomainModelName)
                    {
                        var endMetadata = _store.GetSchemaEntity(node.EndSchemaId);
                        end = GetGraphNode(node.EndId, endMetadata) as MemoryGraphNode;
                        if (end == null)
                            throw new InvalidElementException(node.EndId);
                    }

                    // Mise à jour des infos sur les relations propres à un noeud
                    if (node.StartId == node.EndId)
                    {
                        start.RemoveEdge(node.Id, Direction.Both);
                        UpdateNode(start, startMetadata);
                    }
                    else
                    {
                        start.RemoveEdge(node.Id, Direction.Outgoing);
                        UpdateNode(start, startMetadata);

                        // Relation uni-directionnelle entre domaine.
                        if (end != null)
                        {
                            end.RemoveEdge(node.Id, Direction.Incoming);
                            var endMetadata = _store.GetSchemaEntity(node.EndSchemaId);
                            UpdateNode(end, endMetadata);
                        }
                    }

                    foreach (var prop in metadata.GetProperties(true))
                    {
                        Storage.RemoveNode(node.Id.CreateAttributeIdentity(prop.Name));
                    }

                    DeferRemoveIndex(metadata, node.Id);

                    CurrentTransaction.UpdateProfiler(p => p.RelationshipsDeleted.Incr());
                    CurrentTransaction.UpdateProfiler(p => p.NumberOfEdges.Dec());
                }
                tx.Commit();
            }
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
            //DebugContract.Requires(Session.Current);

            var source = node as MemoryGraphNode;
            Debug.Assert(source != null, "source != null");

            if ((direction & Direction.Outgoing) == Direction.Outgoing)
            {
                foreach (var info in source.GetEdges(Direction.Outgoing, metadata))
                {
                    var m = metadata ?? _store.GetSchemaRelationship(info.SchemaId);
                    yield return GetGraphNode(info.Id, m);
                }
            }

            if ((direction & Direction.Incoming) == Direction.Incoming)
            {
                foreach (var info in source.GetEdges(Direction.Incoming, metadata))
                {
                    var m = metadata ?? _store.GetSchemaRelationship(info.SchemaId);
                    yield return GetGraphNode(info.Id, m);
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            var evictionPolicy = Storage.EvictionPolicy;
            if (evictionPolicy != null)
                evictionPolicy.ElementEvicted -= OnElementEvicted;

            var disposable = Storage as IDisposable;
            if( disposable !=null)
                disposable.Dispose();

            IndexManager.Dispose();
            DomainModel = null;
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

            return IndexManager.CreateIndex(metaclass, name, unique, propertyNames);
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
            IndexManager.DropIndex(name);
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
            return IndexManager.GetIndex(name);
        }

        private void OnElementEvicted(object sender, ElementEvictedEventArgs e)
        {
            IndexManager.OnElementEvicted(e.Status, (Identity) e.Id);
        }

        internal PropertyValue SetProperty(Identity ownerId, ISchemaElement ownerMetadata, ISchemaProperty property, object value, long? version)
        {
            DebugContract.Requires(ownerId);
            DebugContract.Requires(ownerMetadata);
            DebugContract.Requires(property);
            DebugContract.Requires(Session.Current);

            using (var tx = BeginTransaction())
            {
                var node = GetGraphNode(ownerId, ownerMetadata);
                // Vérification si le owner existe
                if (node == null)
                    throw new InvalidElementException(ownerId);

                var pid = ownerId.CreateAttributeIdentity(property.Name);

                // Recherche si l'attribut existe
                var pnode = Storage.GetNode(pid, property) as MemoryGraphNode;
                if (pnode == null)
                {
                    // N'existe pas encore. On crée l'attribut et une relation avec son owner
                    pnode = new MemoryGraphNode(this.DomainModel.Store, pid, property.Id, NodeType.Property, value: value, version: version ?? 1);
                    Storage.AddNode(pnode, property, ownerId);
                    DeferAddIndex(ownerMetadata, ownerId, property.Name, value);
                    tx.Commit();
                    return new PropertyValue { Value = value, OldValue = property.DefaultValue, CurrentVersion = pnode.Version };
                }

                var oldValue = pnode.Value;
                // TODO
                //if (version != null && pnode.Version != version)
                //{
                //    throw new ConflictException(ownerId, ownerMetadata, property, value, oldValue, version.Value, pnode.Version);
                //}

                if (Equals(oldValue, value))
                {
                    tx.Commit();
                    return new PropertyValue { Value = value, OldValue = oldValue, CurrentVersion = pnode.Version };
                }

                DeferRemoveIndex(ownerMetadata, ownerId, property.Name, oldValue);

                pnode = new MemoryGraphNode(pnode, version ?? (pnode.Version + 1));
                pnode.Value = value;
                Storage.UpdateNode(pnode, property);
                DeferAddIndex(ownerMetadata, ownerId, property.Name, value);

                tx.Commit();
                return new PropertyValue { Value = value, OldValue = oldValue, CurrentVersion = pnode.Version };
            }
        }

        private void UpdateNode(MemoryGraphNode node,ISchemaInfo schema)
        {
            DebugContract.Requires(node, "node");
            DebugContract.Requires(schema, "schema");
            Storage.UpdateNode(node, schema);
        }

        internal ITransaction BeginTransaction()
        {
            DebugContract.Requires(Session.Current);

            var tx = CurrentTransaction;
            if (tx == null)
            {
                tx = new HypergraphTransaction(this);
                CurrentTransaction = tx;
                CurrentTransaction.UpdateProfiler(p => p.NumberOfTransactions.Incr());
            }
            else
            {
                // Nested
                tx.PushNestedTransaction();
            }
            return tx;
        }

        internal void OnTransactionTerminated(HypergraphTransaction transaction)
        {
        }

        internal void DeferRemoveIndex(ISchemaElement metaclass, Identity id, string propertyName = null, object key = null)
        {
            DebugContract.Requires(metaclass);
            DebugContract.Requires(id);

            var tx = CurrentTransaction;
            if (tx != null)
                tx.RemoveFromIndex(metaclass, id, propertyName, key);
        }

        internal void DeferAddIndex(ISchemaElement metaclass, Identity id, string propertyName = null, object key = null)
        {
            DebugContract.Requires(metaclass);
            DebugContract.Requires(id);

            var tx = CurrentTransaction;
            if (tx != null)
                tx.AddToIndex(metaclass, id, propertyName, key);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates execute query in this collection.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <param name="option">
        ///  The option.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process execute query in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<QueryNodeResult> ExecuteQuery(Query query, MergeOption option)
        {
            yield break;
            //if (_cache != this)
            //{
            //    foreach (var result in this._cache.Load(metaclass, option, query))
            //    {
            //        yield return result;
            //    }
            //}

            //foreach (var node in Storage.GetAll(metaclass is IMetaRelationship ? GraphElementType.Edge : GraphElementType.Node))
            //{
            //    var nodeMetaClass = node.MetaClassId == metaclass.Id ? metaclass : _store.GetMetadata(node.MetaClassId);
            //    var properties = new Dictionary<IMetaProperty, PropertyInfo>();
            //    foreach (var prop in metaclass.GetProperties(true))
            //    {
            //        properties.Add(prop, GetPropertyValue(node.Id, nodeMetaClass, prop));
            //    }
            //    var result = new NodeQueryResult(node, properties);
            //    yield return result;
            //}
        }
    }
}