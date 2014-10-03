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
using System.Collections.Generic;
using Hyperstore.Modeling.HyperGraph;
using System.Linq;
using Hyperstore.Modeling.Statistics;
using System.Diagnostics;
using System.Threading.Tasks;
using Hyperstore.Modeling.Domain;
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
        /// <param name="services">
        ///  The services.
        /// </param>
        /// <param name="statisticCounterName">
        ///  (Optional) name of the statistic counter.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected AbstractGraphAdapter(IServicesContainer services, string statisticCounterName = null)
        {
            Contract.Requires(services, "services");
            _statisticCounterName = statisticCounterName ?? this.GetType().Name;
            Services = services;
            Store = services.Resolve<IHyperstore>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the services container.
        /// </summary>
        /// <value>
        ///  The services container.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IServicesContainer Services { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store { get; private set; }

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

            if (_sessionSubscription == null && this is IPersistenceGraphAdapter)
            {
                _sessionSubscription = domainModel.Events.SessionCompleted.Subscribe(e =>
                {
                    OnSesssionCompleted(e);
                });
            }

            InitializeCounters();

            Initialize();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Check initialized.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        protected void CheckInitialized()
        {
            if (_initialized == false)
                throw new HyperstoreException("Adapter must be bounded to a domain.");
        }

        private void InitializeCounters()
        {
            var stat = Services.Resolve<IStatistics>() ?? Statistics.EmptyStatistics.DefaultInstance;
            var domainName = DomainModel != null ? DomainModel.Name : "-";
            StatGetNodes = stat.RegisterCounter(_statisticCounterName, String.Format("#GetNodes {0}", domainName), "GetNodes", StatisticCounterType.Value);
            StatGetNodesAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetNodesAvgTime {0}", domainName), "GetNodesAvgTime", StatisticCounterType.Average);
            StatGetNode = stat.RegisterCounter(_statisticCounterName, String.Format("#GetNode {0}", domainName), "GetNode", StatisticCounterType.Value);
            StatGetNodeAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetNodeAvgTime {0}", domainName), "GetNodeAvgTime", StatisticCounterType.Average);
            StatGetProperty = stat.RegisterCounter(_statisticCounterName, String.Format("#GetProperty {0}", domainName), "GetProperty", StatisticCounterType.Value);
            StatGetPropertyAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetPropertyAvgTime {0}", domainName), "GetPropertyAvgTime", StatisticCounterType.Average);
            StatGetEdges = stat.RegisterCounter(_statisticCounterName, String.Format("#GetEdges {0}", domainName), "GetEdges", StatisticCounterType.Value);
            StatGetEdgesAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("GetEdgesAvgTime {0}", domainName), "GetEdgesAvgTime", StatisticCounterType.Average);
            StatSessionCount = stat.RegisterCounter(_statisticCounterName, String.Format("#Session {0}", domainName), "Session", StatisticCounterType.Value);
            StatSessionAvgTime = stat.RegisterCounter(_statisticCounterName, String.Format("SessionAvgTime {0}", domainName), "SessionAvgTime", StatisticCounterType.Average);
        }

        private void OnSesssionCompleted(ISessionInformation session)
        {
            DebugContract.Requires(session);

            var originId = session.OriginStoreId;
            if (session.IsAborted || !session.Events.Any() || _disposed || (originId != Guid.Empty && originId != Store.Id) || (session.Mode & SessionMode.Loading) == SessionMode.Loading || (session.Mode & SessionMode.LoadingSchema) == SessionMode.LoadingSchema)
                return;

            var elements = session.TrackingData.InvolvedTrackedElements.Where(e => String.Compare(e.DomainName, DomainModel.Name, StringComparison.OrdinalIgnoreCase) == 0 && String.Compare(e.Extension, DomainModel.ExtensionName, StringComparison.OrdinalIgnoreCase) == 0);
            if (!elements.Any())
                return;

            var observer = this as IPersistenceGraphAdapter;
            Debug.Assert(observer != null);

            try
            {
                observer.PersistElements(session, elements);
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
        protected abstract IEnumerable<GraphPropertiesNode> LoadNodes(Query query);

        IEnumerable<GraphPropertiesNode> IGraphAdapter.LoadNodes(Query query)
        {
            Contract.Requires(query, "query");
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

            OnClosed();

            DomainModel = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executed when the adapter is closed
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected void OnClosed()
        {
        }
    }
}