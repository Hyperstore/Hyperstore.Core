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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Statistics;
using Hyperstore.Modeling.Platform;
using Hyperstore.Modeling.HyperGraph.Index;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    /// <summary>
    ///     Transaction utilisé par le MemoryGraphAdapter. Toutes les données manipulées
    ///     au sein de la transaction sont thread safe.
    ///     Il est possible de créer plusieurs transactions imbriquées mais elles ne se comportent que comme une seule
    ///     transaction. Si une se termine mal, toutes les autres seront aussi dans ce cas.
    /// </summary>
    internal sealed class HypergraphTransaction : ITransaction, ISessionEnlistmentNotification
    {
        /// <summary>
        ///     Action devant s'executer si la transaction se termine correctement.
        ///     Evite de mettre à jour des informations (statistiques et index) si la transaction ne se termine pas correctement
        /// </summary>
        private interface IPendingAction
        {
            void Execute(MemoryIndexManager indexManager);
        }

        private class UpdateProfilerAction : IPendingAction
        {
            private readonly Action<DomainStatistics> _action;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="action">
            ///  The action.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public UpdateProfilerAction(Action<DomainStatistics> action)
            {
                DebugContract.Requires(action, "action");
                _action = action;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Executes the given graph.
            /// </summary>
            /// <param name="indexManager">
            ///  The graph.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void Execute(MemoryIndexManager indexManager)
            {
                DebugContract.Requires(indexManager);

                _action(indexManager.DomainModel.Statistics);
            }
        }

        private abstract class IndexAction : IPendingAction
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Specialised constructor for use only by derived classes.
            /// </summary>
            /// <param name="metaclass">
            ///  The metaclass.
            /// </param>
            /// <param name="indexName">
            ///  The name of the index.
            /// </param>
            /// <param name="id">
            ///  The identifier.
            /// </param>
            /// <param name="key">
            ///  The key.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            protected IndexAction(ISchemaElement metaclass, string indexName, Identity id, object key)
            {
                DebugContract.Requires(metaclass);
                DebugContract.Requires(id);
                DebugContract.RequiresNotEmpty(indexName);

                SchemaElement = metaclass;
                Id = id;
                Key = key;
                IndexName = indexName;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the identifier.
            /// </summary>
            /// <value>
            ///  The identifier.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public Identity Id { get; private set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the key.
            /// </summary>
            /// <value>
            ///  The key.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public object Key { get; private set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the name of the index.
            /// </summary>
            /// <value>
            ///  The name of the index.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public string IndexName { get; private set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the metaclass.
            /// </summary>
            /// <value>
            ///  The metaclass.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            protected ISchemaElement SchemaElement { get; private set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Executes the given graph.
            /// </summary>
            /// <param name="indexManager">
            ///  The graph.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public abstract void Execute(MemoryIndexManager indexManager);
        }

        private class AddToIndexAction : IndexAction
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="metaclass">
            ///  The metaclass.
            /// </param>
            /// <param name="indexName">
            ///  Name of the index.
            /// </param>
            /// <param name="id">
            ///  The identifier.
            /// </param>
            /// <param name="key">
            ///  The key.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public AddToIndexAction(ISchemaElement metaclass, string indexName, Identity id, object key) : base(metaclass, indexName, id, key)
            {
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Executes the given graph.
            /// </summary>
            /// <param name="indexManager">
            ///  The graph.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public override void Execute(MemoryIndexManager indexManager)
            {
                DebugContract.Requires(indexManager);

                indexManager.AddToIndex(SchemaElement, IndexName, Id, Key);
            }
        }

        private class RemoveFromIndexAction : IndexAction
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="metaclass">
            ///  The metaclass.
            /// </param>
            /// <param name="indexName">
            ///  Name of the index.
            /// </param>
            /// <param name="id">
            ///  The identifier.
            /// </param>
            /// <param name="key">
            ///  The key.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public RemoveFromIndexAction(ISchemaElement metaclass, string indexName, Identity id, object key) : base(metaclass, indexName, id, key)
            {
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Executes the given graph.
            /// </summary>
            /// <param name="indexManager">
            ///  The graph.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public override void Execute(MemoryIndexManager indexManager)
            {
                DebugContract.Requires(indexManager);

                indexManager.RemoveFromIndex(SchemaElement, IndexName, Id, Key);
            }
        }

        private List<IPendingAction> _pendingActions;
        private readonly Hyperstore.Modeling.HyperGraph.Index.MemoryIndexManager _indexManager;
        private List<IndexAction> _indexActions;
        private readonly Stack<TransactionStatus> _nestedStatus = new Stack<TransactionStatus>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true if aborted.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public bool Aborted;
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="indexManager">
        ///  The graph.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public HypergraphTransaction(MemoryIndexManager indexManager)
        {
            Contract.Requires(indexManager, "indexManager");

            _indexManager = indexManager;

            _pendingActions = new List<IPendingAction>();
            Session.Current.Enlist(this);

            PushNestedTransaction();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is top level.
        /// </summary>
        /// <value>
        ///  true if this instance is top level, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsTopLevel
        {
            get { return _nestedStatus.Count == 1; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Pushes the nested transaction.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void PushNestedTransaction()
        {
            _nestedStatus.Push(TransactionStatus.Active);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Commits this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Commit()
        {
            // Pop / Push car TransactionStatus est une valeur
            var current = _nestedStatus.Pop();
            if (current == TransactionStatus.Active)
                current = TransactionStatus.Committed;
            _nestedStatus.Push(current);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            var current = _nestedStatus.Pop();
            if (current != TransactionStatus.Committed)
                Aborted = true;
        }

        bool ISessionEnlistmentNotification.NotifyPrepare()
        {
            return !Aborted;
        }

        void ISessionEnlistmentNotification.NotifyCommit()
        {
            Debug.Assert(_nestedStatus.Count == 0);
            UpdateProfiler(s => s.NumberOfTransactions.Incr());
            ExecutePendinActions();
        }

        void ISessionEnlistmentNotification.NotifyRollback()
        {
            Aborted = true;
        }

        /// <summary>
        ///     Execution des actions en attente lorsque la transaction s'est correctement terminée
        /// </summary>
        private void ExecutePendinActions()
        {
            // La mise à jour des statistiques
            IEnumerable<IPendingAction> actions = _pendingActions;

            if (_indexActions != null)
                actions = actions.Concat(_indexActions); // La mise à jour des index 

            if (actions.Any())
            {
                PlatformServices.Current.Parallel_ForEach(actions, i => i.Execute(_indexManager));
            }
            _indexActions = null;
            _pendingActions = null;
        }

        internal void AddToIndex(ISchemaElement metaclass, Identity owner, string propertyName, object value)
        {
            DebugContract.Requires(metaclass);
            DebugContract.Requires(owner);

            if (_indexActions == null)
                _indexActions = new List<IndexAction>();

            var indexes = _indexManager.GetIndexDefinitionsFor(metaclass);
            if (indexes == null)
                return;

            foreach (var def in indexes)
            {
                if (def.IsImpactedBy(metaclass, propertyName))
                {
                    var action = new AddToIndexAction(metaclass, def.Index.Name, owner, value);
                    _indexActions.Add(action);
                }
            }
        }

        internal void UpdateProfiler(Action<DomainStatistics> action)
        {
            DebugContract.Requires(action);

            _pendingActions.Add(new UpdateProfilerAction(action));
        }

        internal void RemoveFromIndex(ISchemaElement metaclass, Identity owner, string propertyName, object value)
        {
            DebugContract.Requires(metaclass);
            DebugContract.Requires(owner);

            if (_indexActions == null)
                _indexActions = new List<IndexAction>();

            var indexes = _indexManager.GetIndexDefinitionsFor(metaclass);
            if (indexes == null)
                return;

            foreach (var def in indexes)
            {
                if (def.IsImpactedBy(metaclass, propertyName))
                {
                    var action = new RemoveFromIndexAction(metaclass, def.Index.Name, owner, value);
                    _indexActions.Add(action);
                }
            }
        }

        internal bool IsValidInTransaction(Identity id)
        {
            DebugContract.Requires(id);
            return Session.Current.TrackingData.GetTrackedElementState(id) != TrackingState.Removed;
        }

        internal IEnumerable<Identity> GetPendingIndexFor(string name, object key)
        {
            DebugContract.RequiresNotEmpty(name);

            var set = new HashSet<Identity>();

            foreach (var result in _indexActions.Where(a => a.IndexName == name))
            {
                if (result is RemoveFromIndexAction)
                    set.Remove(result.Id);
                else if (Equals(result.Key, key))
                    set.Add(result.Id);
            }
            return set;
        }
    }
}