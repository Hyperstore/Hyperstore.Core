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
using System.Threading;
using Hyperstore.Modeling.Container;
using Hyperstore.Modeling.Statistics;
using Hyperstore.Modeling.Utils;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    // http://momjian.us/main/writings/pgsql/mvcc.pdf
    /// <summary>
    ///     Gestionnaire transactionnel (MVCC) de données en mémoire
    /// </summary>
    internal sealed class TransactionalMemoryStore : IKeyValueStore, IDisposable
    {
        private enum VacuumStrategy
        {
            RemoveSlots,
            ResetContainer
        }

        #region fields
        private const int VACUUM_EVICTIONS_THROTTLE = 5000;

        private const int defaultInterval = 3;

        private IStatisticCounter _statVaccumSkipped;
        private IStatisticCounter _statVaccumCount;
        private IStatisticCounter _statVaccumAverage;

        private IStatisticCounter _statGeIGraphNode;
        private IStatisticCounter _statUpdateValue;
        private IStatisticCounter _statRemoveValue;
        private IStatisticCounter _statAddValue;

        private Guid __id = Guid.NewGuid();
        private bool _disposed;
        private IHyperstoreTrace _trace;
        private int _vaccumCounter;

        /// <summary>
        ///     Le store est thread-safe.
        /// </summary>
        private readonly ReaderWriterLockSlim _valuesLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     Gestionnaire de transaction
        /// </summary>
        private ITransactionManager _transactionManager;

        /// <summary>
        ///     Stockage des slots
        /// </summary>
        private Dictionary<Identity, SlotList> _values = new Dictionary<Identity, SlotList>(VACUUM_EVICTIONS_THROTTLE);

        /// <summary>
        ///     Gestionnaire des demandes de vaccum
        /// </summary>
        private JobScheduler _jobScheduler;
        private IConcurrentQueue<SlotList> _involvedSlots;

        #endregion

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  In-memory transactional store
        /// </summary>
        /// <remarks>
        ///  Settings : MaxTimeBeforeDeadlockInSeconds, MemoryStoreVacuumIntervalInSeconds (see Setting)
        /// </remarks>
        /// <param name="domainModel">
        ///  The associated domain Model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public TransactionalMemoryStore(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel, "domainModel");

            var services = domainModel.Services;

            // On utilise RX pour empiler les demandes de Vaccum en utilisant le principe du sample. c a d quelque soit le nombre de demande de vaccum, on le lancera que toutes les
            // x secondes.
            var defaultMemoryStoreVacuumIntervalInSeconds = services.GetSettingValue<int?>(Setting.MemoryStoreVacuumIntervalInSeconds);
            var n = defaultMemoryStoreVacuumIntervalInSeconds != null ? defaultMemoryStoreVacuumIntervalInSeconds.Value : defaultInterval;

            Initialize(domainModel.Name, n, services.Resolve<ITransactionManager>(), services.Resolve<IHyperstoreTrace>(), services.Resolve<IStatistics>());
        }

        public TransactionalMemoryStore(string domainModelName, int memoryStoreVacuumIntervalInSeconds, ITransactionManager transactionManager, IHyperstoreTrace trace = null, IStatistics stat = null)
        {
            Initialize(domainModelName, memoryStoreVacuumIntervalInSeconds, transactionManager, trace, stat);
        }

        private void Initialize(string domainModelName, int memoryStoreVacuumIntervalInSeconds, ITransactionManager transactionManager, IHyperstoreTrace trace, IStatistics stat)
        {
            DebugContract.RequiresNotEmpty(domainModelName);
            DebugContract.Requires(transactionManager);

            _trace = trace ?? new EmptyHyperstoreTrace();
            _transactionManager = transactionManager;
            if (memoryStoreVacuumIntervalInSeconds < 0)
                memoryStoreVacuumIntervalInSeconds = defaultInterval;

            _jobScheduler = new JobScheduler(Vacuum, TimeSpan.FromSeconds(memoryStoreVacuumIntervalInSeconds));
            _involvedSlots = PlatformServices.Current.CreateConcurrentQueue<SlotList>();

            if (stat == null)
                stat = EmptyStatistics.DefaultInstance;

            _vaccumCounter = VACUUM_EVICTIONS_THROTTLE;

            _statAddValue = stat.RegisterCounter("MemoryStore", String.Format("#AddValue {0}", domainModelName), domainModelName, StatisticCounterType.Value);
            _statGeIGraphNode = stat.RegisterCounter("MemoryStore", String.Format("#GeIGraphNode {0}", domainModelName), domainModelName, StatisticCounterType.Value);
            _statUpdateValue = stat.RegisterCounter("MemoryStore", String.Format("#UpdateValue {0}", domainModelName), domainModelName, StatisticCounterType.Value);
            _statRemoveValue = stat.RegisterCounter("MemoryStore", String.Format("#RemoveValue {0}", domainModelName), domainModelName, StatisticCounterType.Value);
            _statVaccumCount = stat.RegisterCounter("MemoryStore", String.Format("#Vaccum{0}", domainModelName), domainModelName, StatisticCounterType.Value);
            _statVaccumAverage = stat.RegisterCounter("MemoryStore", String.Format("VaccumAvgTimes{0}", domainModelName), domainModelName, StatisticCounterType.Average);
            _statVaccumSkipped = stat.RegisterCounter("MemoryStore", String.Format("#VaccumSkipped{0}", domainModelName), domainModelName, StatisticCounterType.Value);
        }

        private void NotifyVacuum(SlotList slots)
        {
#if !DEBUG
            if (_jobScheduler != null)
            {
                _involvedSlots.Enqueue(slots);
                _jobScheduler.RequestJob();
            }
#endif
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            _jobScheduler.Dispose();

            // Waiting vaccum complete
            _valuesLock.EnterWriteLock();
            try
            {
                if( _transactionManager is IDisposable)
                    ((IDisposable)_transactionManager).Dispose();
                _transactionManager = null;
            }
            finally
            {
                _valuesLock.ExitWriteLock();
            }
        }


#if DEBUG

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Pour les tests.
        /// </summary>
        /// <returns>
        ///  An int.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int CheckVacuum()
        {
            DisableVacuumBatch();
            Vacuum();
            return _values.Count;
        }

        internal void DisableVacuumBatch()
        {
            if (_jobScheduler != null)
                _jobScheduler.Dispose();
            _jobScheduler = null;
        }
#endif

        /// <summary>
        ///     Execution du vaccum.
        ///     Le vaccum consiste à purger les tuples inutiles (qui sont liés à des transactions qui ne sont plus valides). Ce
        ///     mécanisme est nécessaire quand on utilise MVCC car les tuples ne sont
        ///     jamais supprimés, seules les transactions changent de status.
        ///     Lors du vaccum, la liste des transactions est aussi purgé ainsi que les indexs.
        /// </summary>
        private void Vacuum()
        {
            if (_involvedSlots.IsEmpty)
                return;

            // Abort is store is disposing
            if (!_disposed && _valuesLock.TryEnterWriteLock(2))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    if (_disposed)
                    {
                        _statVaccumSkipped.Incr();
                        // ExitWriteLock in finally
                        return;
                    }

                    // Purge des transactions
                    _transactionManager.Vacuum();

                    // Select strategy : if vacuum evictions greater than x -> replace values dictionary
                    var vaccumStrategy = _vaccumCounter <= 0 ? VacuumStrategy.ResetContainer : VacuumStrategy.RemoveSlots;

                    while (!_involvedSlots.IsEmpty)
                    {
                        SlotList slots;
                        if (_involvedSlots.TryDequeue(out slots))
                        {
                            // Evict elements deleted since a while ( = slot marked as deleted by a committed transaction)
                            foreach (var slot in slots)
                            {
                                if (slot.XMax != null)
                                {
                                    var tx = _transactionManager.GetTransaction(slot.XMax.Value);
                                    // Remove slot
                                    if (tx == null)
                                    {
                                        slot.Id = 0;
                                        if (slot is IDisposable)
                                            ((IDisposable)slot).Dispose();
                                    }
                                }
                            }

                            if (vaccumStrategy == VacuumStrategy.RemoveSlots && slots.Length == 0)
                            {
                                slots.Dispose();
                                _values.Remove(slots.Id);
                                _vaccumCounter--;
                            }
                        }
                    }

                    if (vaccumStrategy == VacuumStrategy.ResetContainer)
                    {
                        _vaccumCounter = VACUUM_EVICTIONS_THROTTLE; // RESET

                        // Création d'un dictionnaire temporaire dans lequel on va copier les valeurs à conserver.
                        // Comme un dictionnaire ne permet pas de récupérer la place qui n'est plus utilisé, on va ainsi optimiser
                        // la mémoire.
                        // On peut se permettre de le faire ici car le vaccum bloque toutes les transactions.
                        var data = new Dictionary<Identity, SlotList>((int)(_values.Count * 0.5));

                        // On parcourt toutes les valeurs
                        foreach (var slots in _values)
                        {
                            if (_disposed)
                                break;

                            // If slot list is empty, it is not adding to the new dictionary
                            if (slots.Value.Length == 0)
                                slots.Value.Dispose();
                            else
                                data.Add(slots.Key, new SlotList(slots.Value));
                        }

                        _values = data;
                    }
                }
                finally
                {
                    sw.Stop();
                    _valuesLock.ExitWriteLock();

                    _statVaccumCount.Incr();
                    _statVaccumAverage.IncrBy(sw.ElapsedMilliseconds);
                }
            }
            else
                _statVaccumSkipped.Incr();
        }

        private CommandContext CreateCommandContext()
        {
            return new CommandContext(_transactionManager);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ajout d'une valeur.
        /// </summary>
        /// <exception cref="DuplicateElementException">
        ///  Thrown when a Duplicate Element error condition occurs.
        /// </exception>
        /// <param name="node">
        ///  .
        /// </param>
        /// <param name="ownerKey">
        ///  (Optional)
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void AddNode(GraphNode node, Identity ownerKey = null)
        {
            DebugContract.Requires(node, "node");

            using (var ctx = CreateCommandContext())
            {
                var currentSlot = new Slot<GraphNode>(node);

                _valuesLock.EnterUpgradeableReadLock();
                try
                {
                    SlotList slots = null;
                    if (!_values.TryGetValue(node.Id, out slots))
                    {
                        // N'existe pas encore. On rajoute
                        slots = new SlotList(node.Id, node.NodeType, ownerKey);
                        _valuesLock.EnterWriteLock();
                        try
                        {
                            _values.Add(node.Id, slots);
                        }
                        finally
                        {
                            _valuesLock.ExitWriteLock();
                        }

                        AddSlot(ctx, slots, currentSlot);
                        // No vaccum notification here since the element is new 
                    }
                    else
                    {
                        if (SelectSlot(node.Id, ctx) != null)
                            throw new DuplicateElementException(node.Id.ToString());

                        if (_values.TryGetValue(node.Id, out slots))
                        {
                            var initialSlot = slots.GetActiveSlot();
                            AddSlot(ctx, slots, currentSlot);
                            if (initialSlot != null)
                            {
                                initialSlot.CMin = ctx.CommandId;
                                initialSlot.XMax = ctx.Transaction.Id;
                            }
                            NotifyVacuum(slots);
                        }
                    }

                    _trace.WriteTrace(TraceCategory.MemoryStore, "Add {0} - {1}", node.Id, node);
                    ctx.Complete();
                }
                finally
                {
                    _statAddValue.Incr();
                    _valuesLock.ExitUpgradeableReadLock();
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Suppression d'une valeur.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="key">
        ///  .
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool RemoveNode(Identity key)
        {
            DebugContract.Requires(key);

            using (var ctx = CreateCommandContext())
            {
                _valuesLock.EnterWriteLock();

                try
                {
                    // En MVCC, la suppression consiste seulement à positionner XMAX avec la transaction en cours.
                    // Si cette transaction est committée, la supression sera valide sinon xmax sera ignoré.
                    SlotList slots;
                    if (_values.TryGetValue(key, out slots))
                    {
                        var currentSlot = slots.GetInSnapshot(ctx);
                        if (currentSlot == null)
                            throw new HypergraphException(string.Format(ExceptionMessages.KeyDoesntExistFormat, key));

                        currentSlot.XMax = ctx.Transaction.Id;
                        currentSlot.CMin = ctx.CommandId;
                        _trace.WriteTrace(TraceCategory.MemoryStore, "Remove {0}", key);
                        _statRemoveValue.Incr();
                        NotifyVacuum(slots);
                        return true;
                    }
                }
                finally
                {
                    ctx.Complete();
                    _valuesLock.ExitWriteLock();
                }
            }

            return false;
        }

        private void AddSlot(CommandContext ctx, SlotList slots, Slot<GraphNode> v)
        {
            DebugContract.Requires(ctx, "ctx");
            DebugContract.Requires(slots, "slots");
            DebugContract.Requires(v, "v");

            v.XMin = ctx.Transaction.Id;
            v.XMax = null;
            v.CMin = ctx.CommandId;
            slots.Add(v);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <returns>
        ///  The value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public GraphNode GetNode(Identity key)
        {
            DebugContract.Requires(key);

            using (var ctx = CreateCommandContext())
            {
                _valuesLock.EnterReadLock();
                try
                {
                    var result = SelectSlot(key, ctx);
                    if (result != null)
                        return result.Value;

                    return default(GraphNode);
                }
                finally
                {
                    ctx.Complete();
                    //                NotifyVacuum();
                    _valuesLock.ExitReadLock();
                    _statGeIGraphNode.Incr();
                }
            }
        }

        public bool Exists(Identity id)
        {
            DebugContract.Requires(id);

            using (var ctx = CreateCommandContext())
            {
                _valuesLock.EnterReadLock();
                try
                {
                    var result = SelectSlot(id, ctx);
                    return result != null;
                }
                finally
                {
                    ctx.Complete();
                    //                NotifyVacuum();
                    _valuesLock.ExitReadLock();
                    _statGeIGraphNode.Incr();
                }
            }
        }

        private Slot<GraphNode> SelectSlot(Identity id, CommandContext ctx)
        {
            DebugContract.Requires(id);

            SlotList slots;
            if (_values.TryGetValue(id, out slots))
            {
                return slots.GetInSnapshot(ctx) as Slot<GraphNode>;
            }

            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Mise à jour d'une valeur.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="node">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void UpdateNode(GraphNode node)
        {
            DebugContract.Requires(node);

            using (var ctx = CreateCommandContext())
            {
                try
                {
                    // Test existence tuple ?
                    SlotList slots = null;
                    _valuesLock.EnterReadLock();
                    try
                    {
                        _values.TryGetValue(node.Id, out slots);
                    }
                    finally
                    {
                        _valuesLock.ExitReadLock();
                    }

                    if (slots != null)
                    {
                        var currentSlot = slots.GetActiveSlot();
                        var newSlot = new Slot<GraphNode>(node);
                        AddSlot(ctx, slots, newSlot);
                        if (currentSlot != null)
                        {
                            currentSlot.CMin = ctx.CommandId;
                            currentSlot.XMax = ctx.Transaction.Id;
                        }

                        NotifyVacuum(slots);
                        Debug.Assert(slots.Count(e => e.XMax == null) == 1);
                        _trace.WriteTrace(TraceCategory.MemoryStore, "Update {0} - {1}", node.Id, node);
                    }
                    else
                        throw new HypergraphException(ExceptionMessages.NotFound + node.Id);
                }
                finally
                {
                    // On valide la transaction dans tous les cas pour qu'elle soit purgée par le vacuum
                    ctx.Complete();
                    _statUpdateValue.Incr();
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Lecture de toutes les valeurs valides.
        /// </summary>
        /// <param name="elementType">
        ///  Type of the element.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<GraphNode> GetAllNodes(NodeType elementType)
        {
            //// Sauvegarde du pointeur sur les valeurs qui sera protégé du vaccum (Le vaccum ne supprime pas mais recré un
            //// tableau pour remplacer l'ancien)
            //var values = _values;
            //var ctx = CreateCommandContext();

            //var iter = values.Where(s => s.Value.ElementType == elementType);

            //foreach (var item in iter)
            //{
            //    Slot<GraphNode> slot;
            //    this._valuesLock.EnterReadLock();
            //    try
            //    {
            //        slot = item.Value.GetInSnapshot(ctx) as Slot<GraphNode>;
            //    }
            //    finally
            //    {
            //        this._valuesLock.ExitReadLock();
            //    }

            //    if (slot != null)
            //        yield return slot.Value;
            //}

            //ctx.Complete();

            // Démarrage d'une transaction afin de pouvoir initialiser le contexte 
            // dans lequel va s'exécuter la lecture.
            using (var ctx = CreateCommandContext())
            {
                // Dans un 1er temps, la méthode utilisait un yield pour renvoyer le résultat à chaque itération.
                // Mais cela générait une erreur si pendant l'itération une valeur était modifiée car il y avait un conflit 
                // avec le _valuesLock (impossible de passer en write car on est en read).
                // Pour éviter cela, on va lire toutes les valeurs et les renvoyer d"un coup.
                // TODO voir si on peut pas faire mieux
                var result = new List<GraphNode>(); // Stockage des résultats de l'itération
                _valuesLock.EnterReadLock();
                try
                {
                    var iter = _values.Values.Where(s => (s.ElementType & elementType) != 0);

                    foreach (var slots in iter)
                    {
                        var slot = slots.GetInSnapshot(ctx) as Slot<GraphNode>;
                        if (slot != null)
                            result.Add(slot.Value);
                    }
                }
                finally
                {
                    // On valide la transaction dans tous les cas pour qu'elle soit purgée par le vacuum
                    ctx.Complete();
                    _valuesLock.ExitReadLock();
                    _statGeIGraphNode.IncrBy(result.Count);
                }

                return result;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Closes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Close()
        {
        }
    }
}