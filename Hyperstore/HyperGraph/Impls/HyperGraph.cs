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
using System.Threading.Tasks;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph.Adapters;
using Hyperstore.Modeling.Ioc;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    internal class HyperGraph : IHyperGraph, IDomainService, IIndexManager
    {
        private Guid __id = Guid.NewGuid();

        #region Enums of HyperGraph (4)

        private readonly IDependencyResolver _resolver;
        private ICacheAdapter _cache;
        private bool _disposed;
        private IDomainModel _domainModel;
        private IQueryGraphAdapter _graphAdapter;
        private IHyperstoreTrace _trace;

        #endregion Enums of HyperGraph (4)

        #region Properties of HyperGraph (4)

        private IHyperstore Store
        {
            get { return _domainModel.Store; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the adapter.
        /// </summary>
        /// <value>
        ///  The adapter.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ICacheAdapter Adapter
        {
            get { return _cache; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel
        {
            get { return _domainModel; }
        }

        #endregion Properties of HyperGraph (4)

        #region Constructors of HyperGraph (1)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public HyperGraph(IDependencyResolver resolver)
        {
            Contract.Requires(resolver, "resolver");

            _resolver = resolver;
        }

        #endregion Constructors of HyperGraph (1)

        #region Methods of HyperGraph (21)

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);
            if (_domainModel != null)
                return;

            _trace = domainModel.Resolve<IHyperstoreTrace>(false) ?? new EmptyHyperstoreTrace();
            _domainModel = domainModel;
            _cache = ResolveGraphAdapter();
            _cache.SetDomain(domainModel);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the element.
        /// </summary>
        /// <param name="id">
        ///  .
        /// </param>
        /// <param name="schemaEntity">
        ///  The meta class.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IGraphNode CreateEntity(Identity id, ISchemaEntity schemaEntity)
        {
            DebugContract.Requires(id, "id");
            DebugContract.Requires(schemaEntity);

            Session.Current.AcquireLock(LockType.Exclusive, id);

            _trace.WriteTrace(TraceCategory.Hypergraph, "Add element {0}", id);
            return _cache.CreateEntity(id, schemaEntity);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a relationship.
        /// </summary>
        /// <param name="id">
        ///  .
        /// </param>
        /// <param name="metaRelationship">
        ///  The meta relationship.
        /// </param>
        /// <param name="startId">
        ///  The start identifier.
        /// </param>
        /// <param name="startSchema">
        ///  The start schema.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="endSchema">
        ///  The end meta class.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IGraphNode CreateRelationship(Identity id, ISchemaRelationship metaRelationship, Identity startId, ISchemaElement startSchema, Identity end, ISchemaElement endSchema)
        {
            DebugContract.Requires(id);
            DebugContract.Requires(metaRelationship);
            DebugContract.Requires(startId);
            DebugContract.Requires(startSchema);
            DebugContract.Requires(end);
            DebugContract.Requires(endSchema);

            _trace.WriteTrace(TraceCategory.Hypergraph, "Add relationship {0} ({1}->{2})", id, startId, end);
            Session.Current.AcquireLock(LockType.Exclusive, id);
            Session.Current.AcquireLock(LockType.Exclusive, startId);
            Session.Current.AcquireLock(LockType.Exclusive, end);

            //                    _statistics.Increment(StatisticCounters.EdgesCreated);
            return _cache.CreateRelationship(id, metaRelationship, startId, startSchema, end, endSchema);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Peut-être une entity ou une relation.
        /// </summary>
        /// <param name="id">
        ///  .
        /// </param>
        /// <param name="metaclass">
        ///  .
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement GetElement(Identity id, ISchemaElement metaclass, bool localOnly)
        {
            Contract.Requires(id, "id");

            var v = _cache.GetGraphNode(id, metaclass, localOnly);
            if (v == null)
                return null;

            var metadata = _domainModel.Store.GetSchemaElement(v.SchemaId);
            if (metaclass != null && metaclass.IsA(metadata))
                metadata = metaclass;

            return (IModelElement)metadata.Deserialize(new SerializationContext(_domainModel, metadata, v));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <param name="id">
        ///  .
        /// </param>
        /// <param name="metaclass">
        ///  .
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelEntity GetEntity(Identity id, ISchemaEntity metaclass, bool localOnly)
        {
            Contract.Requires(id, "id");

            var v = _cache.GetGraphNode(id, metaclass, localOnly);
            if (v == null)
                return null;

            var metadata = _domainModel.Store.GetSchemaEntity(v.SchemaId);
            if (metaclass != null && metaclass.IsA(metadata))
                metadata = metaclass;

            return (IModelEntity)metadata.Deserialize(new SerializationContext(_domainModel, metadata, v));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelEntity> GetEntities(ISchemaEntity metadata, int skip, bool localOnly)
        {
            return GetEntities<IModelEntity>(metadata, skip, localOnly);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element or relationships.
        /// </summary>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelElement> GetElements(ISchemaElement metadata, int skip, bool localOnly)
        {
            ISchemaElement currentMetadata = null;
            var cx = 0;
            foreach (var e in _cache.GetGraphNodes(NodeType.EdgeOrNode, metadata, localOnly))
            {
                if (e == null)
                    continue;

                if (currentMetadata == null || currentMetadata.Id != e.SchemaId)
                    currentMetadata = _domainModel.Store.GetSchemaElement(e.SchemaId);

                if (metadata == null || currentMetadata.IsA(metadata))
                {
                    if (cx++ >= skip)
                    {
                        var ctx = new SerializationContext(_domainModel, currentMetadata, e);
                        yield return (IModelElement)currentMetadata.Deserialize(ctx);
                    }
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetEntities<T>(ISchemaEntity metadata, int skip, bool localOnly) where T : IModelEntity
        {
            ISchemaEntity currentMetadata = null;
            var cx = 0;
            foreach (var e in _cache.GetGraphNodes(NodeType.Node, metadata, localOnly))
            {
                if (e == null)
                    continue;

                if (currentMetadata == null || currentMetadata.Id != e.SchemaId)
                    currentMetadata = _domainModel.Store.GetSchemaEntity(e.SchemaId);

                if (metadata == null || currentMetadata.IsA(metadata))
                {
                    if (cx++ >= skip)
                    {
                        var ctx = new SerializationContext(_domainModel, currentMetadata, e);
                        yield return (T)currentMetadata.Deserialize(ctx);
                    }
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <param name="id">
        ///  .
        /// </param>
        /// <param name="metaclass">
        ///  .
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship GetRelationship(Identity id, ISchemaRelationship metaclass, bool localOnly)
        {
            DebugContract.Requires(id);
            return (IModelRelationship)GetElement(id, metaclass, localOnly); // TODO voir si cela a un interet
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship metadata, IModelElement start, IModelElement end, int skip, bool localOnly)
        {
            return GetRelationships<IModelRelationship>(metadata, start, end, skip, localOnly);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetRelationships<T>(ISchemaRelationship metadata, IModelElement start, IModelElement end, int skip, bool localOnly) where T : IModelRelationship
        {
            var currentMetadata = metadata;

            var cx = 0;
            if (start != null)
            {
                var node = _cache.GetGraphNode(start.Id, start.SchemaInfo, localOnly);
                if (node != null)
                {
                    //using (Session.Current.AcquireLock(LockType.Shared, start.Id))
                    {
                        foreach (var edge in _cache.GetEdges(node, Direction.Outgoing, metadata, localOnly))
                        {
                            if (edge == null)
                                continue;

                            if (end == null || end.Id == edge.EndId)
                            {
                                if (cx++ < skip)
                                    continue;

                                if (currentMetadata == null || currentMetadata.Id != edge.SchemaId)
                                    currentMetadata = _domainModel.Store.GetSchemaRelationship(edge.SchemaId);

                                var ctx = new SerializationContext(_domainModel, currentMetadata, edge);
                                yield return (T)currentMetadata.Deserialize(ctx);
                            }
                        }
                    }
                }
            }
            else if (end != null)
            {
                var node = _cache.GetGraphNode(end.Id, end.SchemaInfo, localOnly);
                if (node != null)
                {
                    //using (Session.Current.AcquireLock(LockType.Shared, end.Id))
                    {
                        foreach (var edge in _cache.GetEdges(node, Direction.Incoming, metadata, localOnly))
                        {
                            if (cx++ < skip)
                                continue;

                            if (currentMetadata == null || currentMetadata.Id != edge.SchemaId)
                                currentMetadata = _domainModel.Store.GetSchemaRelationship(edge.SchemaId);

                            var ctx = new SerializationContext(_domainModel, currentMetadata, edge);
                            yield return (T)currentMetadata.Deserialize(ctx);
                        }
                    }
                }
            }
            else
            {
                foreach (var e in _cache.GetGraphNodes(NodeType.Edge, metadata, localOnly))
                {
                    if (e == null || e.NodeType != NodeType.Edge) // Ce n'est pas une relation
                        continue;

                    if (cx++ < skip)
                        continue;

                    if (currentMetadata == null || currentMetadata.Id != e.SchemaId)
                        currentMetadata = _domainModel.Store.GetSchemaRelationship(e.SchemaId);

                    if (metadata == null || currentMetadata.IsA(metadata))
                    {
                        var ctx = new SerializationContext(_domainModel, currentMetadata, e);
                        yield return (T)currentMetadata.Deserialize(ctx);
                    }
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <param name="option">
        ///  The option.
        /// </param>
        /// <returns>
        ///  The element with graph provider asynchronous.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Task<int> LoadElementWithGraphProviderAsync(Query query, MergeOption option)
        {
            var tcs = new TaskCompletionSource<int>();
            if (_graphAdapter == null)
            {
                tcs.TrySetResult(0);
                return tcs.Task;
            }

            var cx = 0;
            try
            {
                var q = _graphAdapter.LoadNodes(query);
                using (var session = this.Store.BeginSession(new SessionConfiguration { Mode = SessionMode.Loading | SessionMode.SkipConstraints }))
                {
                    foreach (var result in q)
                    {
                        if (Session.Current != null && Session.Current.TrackingData.GetTrackingElementState(result.Node.Id) == TrackingState.Removed)
                            continue;
                        cx++;
                        var newInCache = false;
                        var nodeMetaclass = result.SchemaInfo;

                        // Si ce noeud n'existe pas dans le cache, on le met
                        var node = _cache.GetGraphNode(result.Node.Id, nodeMetaclass, true) as MemoryGraphNode;
                        if (node == null)
                        {
                            if (result.Node.NodeType == NodeType.Edge)
                            {
                                node =
                                        _cache.CreateRelationship(result.Node.Id, nodeMetaclass as ISchemaRelationship, result.Node.StartId, _cache.DomainModel.Store.GetSchemaElement(result.Node.StartSchemaId), result.Node.EndId,
                                                _cache.DomainModel.Store.GetSchemaElement(result.Node.EndSchemaId)) as MemoryGraphNode;
                            }
                            else
                                node = _cache.CreateEntity(result.Node.Id, nodeMetaclass as ISchemaEntity) as MemoryGraphNode;
                            newInCache = true;
                        }

                        // TODO
                        //if (option == MergeOption.AppendOnly && newInCache || option == MergeOption.OverwriteChanges)
                        //{
                        //    foreach (var edge in this._graphAdapter.GetEdges(node, Direction.Outgoing, null))
                        //    {
                        //        node.AddEdge(edge.Id, edge.MetaClassId, edge.End, edge.EndMetadata, Direction.Outgoing);
                        //    }

                        //    foreach (var edge in this._adapter.GetEdges(node, Direction.Incoming))
                        //    {
                        //        node.AddEdge(edge.Id, edge.MetaClassId, edge.End, edge.EndMetadata, Direction.Incoming);
                        //    }
                        //}

                        var ctx = new SerializationContext(_domainModel, result.SchemaInfo, result.Node);
                        var mel = (IModelElement)result.SchemaInfo.Deserialize(ctx);
                        if (mel != null)
                        {
                            if (result.Properties != null)
                            {
                                foreach (var property in result.Properties)
                                {
                                    // Mise à jour des propriétés lues
                                    if (option == MergeOption.AppendOnly && newInCache || option == MergeOption.OverwriteChanges)
                                        _cache.SetPropertyValue(mel, property.Key, property.Value.Value, property.Value.CurrentVersion);
                                    else if (option == MergeOption.PreserveChanges)
                                    {
                                        if (_cache.GetPropertyValue(node.Id, nodeMetaclass, property.Key) == null)
                                            _cache.SetPropertyValue(mel, property.Key, property.Value.Value, property.Value.CurrentVersion);
                                    }
                                }
                            }
                        }
                    }
                    session.AcceptChanges();
                }
                tcs.TrySetResult(cx);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the element.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  .
        /// </param>
        /// <param name="metaClass">
        ///  The meta class.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  true to throw exception if not exists.
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool RemoveEntity(Identity id, ISchemaEntity metaClass, bool throwExceptionIfNotExists, bool localOnly)
        {
            DebugContract.Requires(id);
            DebugContract.Requires(metaClass);
            DebugContract.Requires(Session.Current);

            if (metaClass is ISchemaRelationship)
                throw new Exception(ExceptionMessages.UseRemoveRelationshipToRemoveRelationship);

            Session.Current.AcquireLock(LockType.Exclusive, id);

            var node = _cache.GetGraphNode(id, metaClass, localOnly);
            if (node == null)
            {
                if (!throwExceptionIfNotExists)
                    return false;

                throw new InvalidElementException(id);
            }

            _trace.WriteTrace(TraceCategory.Hypergraph, "Remove element {0}", id);

            RemoveDependencies(node, metaClass, localOnly);

            _cache.RemoveEntity(node, metaClass);
            return true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  .
        /// </param>
        /// <param name="metaClass">
        ///  The meta class.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  true to throw exception if not exists.
        /// </param>
        /// <param name="localOnly">
        ///  .
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool RemoveRelationship(Identity id, ISchemaRelationship metaClass, bool throwExceptionIfNotExists, bool localOnly)
        {
            DebugContract.Requires(id);
            DebugContract.Requires(metaClass);
            DebugContract.Requires(Session.Current);

            Session.Current.AcquireLock(LockType.Exclusive, id);

            var edge = _cache.GetGraphNode(id, metaClass, localOnly);
            if (edge == null || edge.NodeType != NodeType.Edge)
            {
                if (!throwExceptionIfNotExists)
                    return false;
                throw new InvalidElementException(id);
            }

            Session.Current.AcquireLock(LockType.Exclusive, edge.StartId);
            Session.Current.AcquireLock(LockType.Exclusive, edge.EndId);

            _trace.WriteTrace(TraceCategory.Hypergraph, "Remove relationship {0}", id);
            RemoveDependencies(edge, metaClass, localOnly);

            _cache.RemoveRelationship(edge, metaClass);
            return true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the attribute.
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

            return _cache.GetPropertyValue(ownerId, ownerMetadata, property);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets the attribute.
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
        ///  [out] The version.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public PropertyValue SetPropertyValue(IModelElement owner, ISchemaProperty property, object value, long? version)
        {
            DebugContract.Requires(owner);
            DebugContract.Requires(property);
            DebugContract.Requires(Session.Current);

            _trace.WriteTrace(TraceCategory.Hypergraph, "{0}.{1} = {2}", owner, property.Name, value);
          //  Session.Current.AcquireLock(LockType.Exclusive, owner.Id.CreateAttributeIdentity(property.Name));

            return _cache.SetPropertyValue(owner, property, value, version);            
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the index.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="metaclass">
        ///  .
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

            if (Adapter is IIndexManager)
                return ((IIndexManager)Adapter).CreateIndex(metaclass, name, unique, propertyNames);
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Drops the index.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="name">
        ///  The name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void DropIndex(string name)
        {
            DebugContract.RequiresNotEmpty(name);

            if (Adapter is IIndexManager)
                ((IIndexManager)Adapter).DropIndex(name);
            else
                throw new NotImplementedException();
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

            if (Adapter is IIndexManager)
                return ((IIndexManager)Adapter).GetIndex(name);
            return null;
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
            if (_cache is IDisposable)
                ((IDisposable)_cache).Dispose();
            _disposed = true;
            _domainModel = null;
            _resolver.Dispose();
        }

        private void RemoveDependencies(IGraphNode node, ISchemaElement metaClass, bool localOnly)
        {
            DebugContract.Requires(node);
            DebugContract.Requires(metaClass);
            DebugContract.Requires(Session.Current);

            if (metaClass == null)
                metaClass = _domainModel.Store.GetSchemaElement(node.SchemaId);

            // Dans le cas de la suppression d'un noeud, on ne supprime que les relations entrantes.
            // Pour les relations sortantes de type IsEmbedded, on va supprimer le noeud opposé qui va lui-même supprimer
            // ces relations entrantes.            
            var commands = new List<IDomainCommand>();

            // Suppression des relations entrantes
            foreach (var incoming in _cache.GetEdges(node, Direction.Incoming, null, localOnly))
            {
                if (incoming == null)
                    continue;

                Debug.Assert(_domainModel.Name == incoming.Id.DomainModelName);

                // Génére la commande de suppression.
                commands.Add(new RemoveRelationshipCommand(_domainModel, incoming.Id, incoming.SchemaId));
            }

            // Gestion des relations sortantes
            // On sait que ces relations sont toujours dans le graphe courant mais peut-être pas le noeud terminal
            foreach (var outgoing in _cache.GetEdges(node, Direction.Outgoing, null, localOnly))
            {
                if (outgoing == null)
                    continue;

                // On va générer une commande de suppression mais sur le noeud opposé
                var metaRel = Store.GetSchemaRelationship(outgoing.SchemaId);
                if (metaRel.IsEmbedded)
                {
                    var outv = outgoing.EndId;
                    if (outv != node.Id)
                    {
                        var dm = _domainModel;
                        if (dm.Name != outv.DomainModelName)
                        {
                            dm = Store.GetDomainModel(outv.DomainModelName);
                            // Si le noeud est dans un domaine diffèrent, c'est à nous de supprimer la relation entrante car
                            // elle n'a pas été mise à jour sur le noeud distant
                            commands.Add(new RemoveRelationshipCommand(_domainModel, outgoing.Id, outgoing.SchemaId, false));
                        }
                        // Suppression du noeud distant
                        commands.Add(new RemoveEntityCommand(dm, outv, outgoing.EndSchemaId, false));
                    }
                }
                else
                    commands.Add(new RemoveRelationshipCommand(_domainModel, outgoing.Id, outgoing.SchemaId));
            }

            // Suppression des propriétés 
            foreach (var prop in metaClass.GetProperties(true))
            {
                // Pour chaque propriété qui n'est pas une relation, on va générer un événement spécifique
                if (prop.PropertySchema is ISchemaRelationship)
                    continue;

                // TODO - Est ce la bonne solution ?
                // Verif si le noeud existe pour ne générer la commande que sur les noeuds existants. On fait 
                // cela car c'est le cas pour les propriétés de type relation
                var pnode = _cache.GetPropertyValue(node.Id, metaClass, prop);
                if (pnode != null)
                    commands.Add(new RemovePropertyCommand(_domainModel, node.Id, metaClass, prop));
            }

            if (commands.Count > 0)
                Session.Current.Execute(commands.ToArray());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve graph adapter.
        /// </summary>
        /// <returns>
        ///  An ICacheAdapter.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ICacheAdapter ResolveGraphAdapter()
        {
            _graphAdapter = _resolver.Resolve<IGraphAdapter>() as IQueryGraphAdapter;

            var cache = _resolver.Resolve<ICacheAdapter>() ?? new MemoryGraphAdapter(_resolver);

            if (_graphAdapter != null)
                cache = new CacheAdapter(_resolver, _graphAdapter);
            return cache;
        }

        #endregion Methods of HyperGraph (21)
    }
}