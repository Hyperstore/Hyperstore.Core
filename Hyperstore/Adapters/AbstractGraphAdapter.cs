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
using Hyperstore.Modeling.HyperGraph;
using System.Linq;
using Hyperstore.Modeling.Statistics;
#endregion

namespace Hyperstore.Modeling.Adapters
{

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An abstract graph adapter.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IGraphAdapter"/>
    /// <seealso cref="T:Hyperstore.Modeling.IQueryGraphAdapter"/>
    /// <seealso cref="T:System.IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public abstract class AbstractGraphAdapter : IDisposable, IDomainService, IGraphAdapter
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
        protected AbstractGraphAdapter(IDependencyResolver resolver, string statisticCounterName = null)
        {
            Contract.Requires(resolver, "resolver");
            DependencyResolver = resolver;
            _statisticCounterName = statisticCounterName ?? this.GetType().Name;
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
            if (_sessionSubscription == null)
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
            else
                stat = Statistics.EmptyStatistics.DefaultInstance;

            StatGetNodes = stat.RegisterCounter(_statisticCounterName, String.Format("#GetNodes {0}", domainModel.Name), "GetNodes", StatisticCounterType.Value);
            StatGetNodesAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetNodesAvgTime {0}", domainModel.Name), "GetNodesAvgTime", StatisticCounterType.Average);
            StatGetNode = stat.RegisterCounter(_statisticCounterName, String.Format("#GetNode {0}", domainModel.Name), "GetNode", StatisticCounterType.Value);
            StatGetNodeAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetNodeAvgTime {0}", domainModel.Name), "GetNodeAvgTime", StatisticCounterType.Average);
            StatGetProperty = stat.RegisterCounter(_statisticCounterName, String.Format("#GetProperty {0}", domainModel.Name), "GetProperty", StatisticCounterType.Value);
            StatGetPropertyAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetPropertyAvgTime {0}", domainModel.Name), "GetPropertyAvgTime", StatisticCounterType.Average);
            StatGetEdges = stat.RegisterCounter(_statisticCounterName, String.Format("#GetEdges {0}", domainModel.Name), "GetEdges", StatisticCounterType.Value);
            StatGetEdgesAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetEdgesAvgTime {0}", domainModel.Name), "GetEdgesAvgTime", StatisticCounterType.Average);

            StatSessionCount = stat.RegisterCounter(_statisticCounterName, String.Format("#Session {0}", DomainModel.Name), "Session", StatisticCounterType.Value);
            StatSessionAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("SessionAvgTime {0}", DomainModel.Name), "SessionAvgTime", StatisticCounterType.Average);

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

        IEnumerable<QueryNodeResult> IGraphAdapter.LoadNodes(Query query)
        {
            return LoadNodes(query);
        }

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
    }
}