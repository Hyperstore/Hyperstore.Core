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
using System.Threading.Tasks;
using Hyperstore.Modeling.HyperGraph;
using System.Linq;
using Hyperstore.Modeling.Statistics;
#endregion

namespace Hyperstore.Modeling.Adapters
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Values that represent AdapterCapability.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    [PublicAPI]
    public enum AdapterCapability
    {
        /// <summary>
        ///     An enum constant representing the disabled option.
        /// </summary>
        Disabled = 0,
        /// <summary>
        ///     An enum constant representing the query option.
        /// </summary>
        Query = 1,
        /// <summary>
        ///     An enum constant representing the update option.
        /// </summary>
        Update = 2,
        /// <summary>
        ///     An enum constant representing the query and update option.
        /// </summary>
        QueryAndUpdate = 3
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An abstract graph adapter.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IGraphAdapter"/>
    /// <seealso cref="T:Hyperstore.Modeling.IQueryGraphAdapter"/>
    /// <seealso cref="T:System.IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public abstract class AbstractGraphAdapter : IGraphAdapter, IQueryGraphAdapter, IDisposable
    {
        private IDisposable _sessionSubscription;
        private bool _disposed;
        private bool _initialized;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get nodes.
        /// </summary>
        /// <value>
        ///  The stat get nodes.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetNodes { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get nodes average time.
        /// </summary>
        /// <value>
        ///  The stat get nodes average time.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetNodesAvgTime { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get node.
        /// </summary>
        /// <value>
        ///  The stat get node.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetNode { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get node average time.
        /// </summary>
        /// <value>
        ///  The stat get node average time.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetNodeAvgTime { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get property.
        /// </summary>
        /// <value>
        ///  The stat get property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetProperty { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get property average time.
        /// </summary>
        /// <value>
        ///  The stat get property average time.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetPropertyAvgTime { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get edges.
        /// </summary>
        /// <value>
        ///  The stat get edges.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetEdges { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat get edges average time.
        /// </summary>
        /// <value>
        ///  The stat get edges average time.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatGetEdgesAvgTime { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of stat sessions.
        /// </summary>
        /// <value>
        ///  The number of stat sessions.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatSessionCount { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the stat session average time.
        /// </summary>
        /// <value>
        ///  The stat session average time.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IStatisticCounter StatSessionAvgTime { get; private set; }
        private readonly string _statisticCounterName;
        private readonly AdapterCapability _capability;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <param name="capability">
        ///  (Optional) the capability.
        /// </param>
        /// <param name="statisticCounterName">
        ///  (Optional) name of the statistic counter.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected AbstractGraphAdapter(IDependencyResolver resolver, AdapterCapability capability = AdapterCapability.QueryAndUpdate, string statisticCounterName = null)
        {
            Contract.Requires(resolver, "resolver");
            DependencyResolver = resolver;
            _statisticCounterName = statisticCounterName ?? this.GetType().Name;
            _capability = capability;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the dependency resolver.
        /// </summary>
        /// <value>
        ///  The dependency resolver.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDependencyResolver DependencyResolver { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);
            DomainModel = domainModel;
            if (_initialized)
                return;

            _initialized = true;
            if (_sessionSubscription == null && SupportsPersistence())
            {
                _sessionSubscription = domainModel.Events.SessionCompleted.Subscribe(e =>
                {
                    OnSesssionCompleted(e);
                });
            }

            Initialize();

            IStatistics stat = null;
            if (DependencyResolver != null)
                stat = DependencyResolver.Resolve<IStatistics>();

            StatGetNodes = stat.RegisterCounter(_statisticCounterName, String.Format("#GetNodes {0}-{1}", domainModel.Name ), "GetNodes", StatisticCounterType.Value);
            StatGetNodesAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetNodesAvgTime {0}-{1}", domainModel.Name ), "GetNodesAvgTime", StatisticCounterType.Average);
            StatGetNode = stat.RegisterCounter(_statisticCounterName, String.Format("#GetNode {0}-{1}", domainModel.Name ), "GetNode", StatisticCounterType.Value);
            StatGetNodeAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetNodeAvgTime {0}-{1}", domainModel.Name ), "GetNodeAvgTime", StatisticCounterType.Average);
            StatGetProperty = stat.RegisterCounter(_statisticCounterName, String.Format("#GetProperty {0}-{1}", domainModel.Name ), "GetProperty", StatisticCounterType.Value);
            StatGetPropertyAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetPropertyAvgTime {0}-{1}", domainModel.Name ), "GetPropertyAvgTime", StatisticCounterType.Average);
            StatGetEdges = stat.RegisterCounter(_statisticCounterName, String.Format("#GetEdges {0}-{1}", domainModel.Name ), "GetEdges", StatisticCounterType.Value);
            StatGetEdgesAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetEdgesAvgTime {0}-{1}", domainModel.Name ), "GetEdgesAvgTime", StatisticCounterType.Average);

            StatSessionCount = stat.RegisterCounter(_statisticCounterName, String.Format("#Session {0}-{1}", DomainModel.Name ), "Session", StatisticCounterType.Value);
            StatSessionAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("SessionAvgTime {0}-{1}", DomainModel.Name ), "SessionAvgTime", StatisticCounterType.Average);

        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets wether this provider supports persistence.
        /// </summary>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual bool SupportsPersistence()
        {
            return (_capability & AdapterCapability.Update) == AdapterCapability.Update && this is IPersistenceGraphAdapter;
        }

        private void OnSesssionCompleted(ISessionInformation session)
        {
            DebugContract.Requires(session);

            var originId = session.OriginStoreId;
            if (session.IsAborted || !session.Events.Any() || _disposed || (originId != Guid.Empty && originId != DomainModel.Store.Id))
                return;

            var observer = this as IPersistenceGraphAdapter;
            if (observer == null)
                return;

            try
            {
                observer.PersistSessionElements(session);
            }
            catch (Exception ex)
            {
                session.Log(new DiagnosticMessage(MessageType.Error, ExceptionMessages.Diagnostic_ErrorInPersistenceAdapter, observer.GetType().FullName, null, ex));
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the associated domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Read a node (or edge) from the graph.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <returns>
        ///  Le noeud et ses noeuds associés ou null.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected abstract QueryNodeResult GetNode(Identity id, ISchemaElement metaclass);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Load.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process load nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected abstract IEnumerable<QueryNodeResult> LoadNodes(Query query);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the edges.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="includeProperties">
        ///  true to include, false to exclude the properties.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the edges in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected abstract IEnumerable<QueryNodeResult> GetEdges(Identity id, Direction direction, ISchemaRelationship metadata, bool includeProperties);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the nodes.
        /// </summary>
        /// <param name="elementType">
        ///  Type of the element.
        /// </param>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected abstract IEnumerable<QueryNodeResult> GetNodes(NodeType elementType, ISchemaElement metadata);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property.
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
        protected abstract PropertyValue GetPropertyValue(Identity ownerId, ISchemaElement ownerMetaclass, ISchemaProperty property);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Initialize()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
            if (_sessionSubscription != null)
            {
                _sessionSubscription.Dispose();
                _sessionSubscription = null;
            }
            DomainModel = null;
        }


        QueryNodeResult IQueryGraphAdapter.GetNode(Identity id, ISchemaElement schemaElement)
        {
            if ((_capability & AdapterCapability.Query) != AdapterCapability.Query)
                return null;
            return GetNode(id, schemaElement);
        }

        IEnumerable<QueryNodeResult> IQueryGraphAdapter.LoadNodes(Query query)
        {
            if ((_capability & AdapterCapability.Query) != AdapterCapability.Query)
                return Enumerable.Empty<QueryNodeResult>();

            return LoadNodes(query);
        }

        IEnumerable<QueryNodeResult> IQueryGraphAdapter.GetEdges(Identity id, Direction direction, ISchemaRelationship schemaRelationship, bool includeProperties)
        {
            if ((_capability & AdapterCapability.Query) != AdapterCapability.Query)
                return Enumerable.Empty<QueryNodeResult>();
            return GetEdges(id, direction, schemaRelationship, includeProperties);
        }

        IEnumerable<QueryNodeResult> IQueryGraphAdapter.GetNodes(NodeType elementType, ISchemaElement schemaElement)
        {
            if ((_capability & AdapterCapability.Query) != AdapterCapability.Query)
                return Enumerable.Empty<QueryNodeResult>();
            return GetNodes(elementType, schemaElement);
        }

        PropertyValue IQueryGraphAdapter.GetPropertyValue(Identity ownerId, ISchemaElement schemaElement, ISchemaProperty schemaProperty)
        {
            if ((_capability & AdapterCapability.Query) != AdapterCapability.Query)
                return null;
            return GetPropertyValue(ownerId, schemaElement, schemaProperty);
        }
    }
}