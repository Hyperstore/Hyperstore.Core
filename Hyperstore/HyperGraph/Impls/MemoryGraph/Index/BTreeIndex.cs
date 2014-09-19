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
using System.Linq;
using System.Threading;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    internal sealed class BTreeIndex : IIndex
    {
        private readonly WeakReference _graph;
        /// <summary>
        ///     Btree (value, list of id)
        /// </summary>
        private readonly SortedDictionary<object, HashSet<Identity>> _index = new SortedDictionary<object, HashSet<Identity>>();
        private readonly Dictionary<Identity, HashSet<object>> _keysById = new Dictionary<Identity, HashSet<object>>();
        private readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();

        private readonly bool _unique;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="graph">
        ///  The graph.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="unique">
        ///  true to unique.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public BTreeIndex(IHyperGraph graph, string name, bool unique)
        {
            DebugContract.Requires(graph);
            DebugContract.RequiresNotEmpty(name);

            _graph = new WeakReference(graph);
            Name = name;
            _unique = unique;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name.
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Name { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [is unique].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [is unique]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsUnique
        {
            get { return _unique; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the specified key.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Identity Get(object key)
        {
            if (key == null)
                return null;

            using (var session = EnsuresRunInSession())
            {
                HypergraphTransaction tx = null;
                var graph = _graph.Target as HyperGraph;
                if (graph != null)
                    tx = graph.CurrentTransaction;

                _sync.EnterReadLock();
                try
                {
                    HashSet<Identity> list;
                    if (_index.TryGetValue(key, out list))
                    {
                        foreach (var id in list)
                        {
                            if (tx == null || tx.IsValidInTransaction(id))
                                return id;
                        }
                    }

                    // Il se peut que l'index ne soit pas encore mis à jour dans le contexte d'une transaction.
                    // On va voir si ce n'est pas le cas
                    if (tx != null)
                    {
                        return tx.GetPendingIndexFor(Name, key)
                                .FirstOrDefault();
                    }
                    return null;
                }
                finally
                {
                    _sync.ExitReadLock();
                    if (session != null)
                        session.AcceptChanges();
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all.
        /// </summary>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<Identity> GetAll(int skip = 0)
        {
            HypergraphTransaction tx = null;

            using (var session = EnsuresRunInSession())
            {
                var graph = _graph.Target as HyperGraph;
                if (graph != null)
                    tx = graph.CurrentTransaction;

                _sync.EnterReadLock();
                try
                {
                    var cx = 0;
                    var result = new List<Identity>();
                    foreach (var key in _index.Keys)
                    {
                        HashSet<Identity> list;
                        if (_index.TryGetValue(key, out list))
                        {
                            foreach (var id in list)
                            {
                                if (tx == null || tx.IsValidInTransaction(id))
                                {
                                    if (cx++ >= skip)
                                        result.Add(id);
                                }
                            }
                        }

                        if (tx != null && cx <= skip)
                        {
                            foreach (var id in tx.GetPendingIndexFor(Name, key))
                            {
                                if (cx++ >= skip)
                                    result.Add(id);
                            }
                        }
                    }
                    return result;
                }
                finally
                {
                    _sync.ExitReadLock();
                    if (session != null)
                        session.AcceptChanges();
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<Identity> GetAll(object key, int skip)
        {
            if (key == null)
                return null;

            using (var session = EnsuresRunInSession())
            {
                HypergraphTransaction tx = null;
                var graph = _graph.Target as HyperGraph;
                if (graph != null)
                    tx = graph.CurrentTransaction;

                _sync.EnterReadLock();
                try
                {
                    var cx = 0;
                    var result = new List<Identity>();
                    HashSet<Identity> list;
                    if (_index.TryGetValue(key, out list))
                    {
                        foreach (var id in list)
                        {
                            if (tx == null || tx.IsValidInTransaction(id))
                            {
                                if (cx++ >= skip)
                                    result.Add(id);
                            }
                        }
                    }

                    if (tx != null && cx <= skip)
                    {
                        foreach (var id in tx.GetPendingIndexFor(Name, key))
                        {
                            if (cx++ >= skip)
                                result.Add(id);
                        }
                    }
                    return result;
                }
                finally
                {
                    _sync.ExitReadLock();
                    if (session != null)
                        session.AcceptChanges();
                }
            }
        }

        internal void Add(Identity id, object key)
        {
            DebugContract.Requires(id);

            _sync.EnterWriteLock();
            try
            {
                HashSet<Identity> list;
                if (!_index.TryGetValue(key, out list))
                    _index[key] = list = new HashSet<Identity>();
                else if (_unique)
                    throw new UniqueConstraintException(String.Format("Unique constraint failed for index {0}, id={1}, key={2}", Name, id, key));

                if (!list.Add(id))
                    throw new Exception(ExceptionMessages.IndexInsertionFailed);

                HashSet<object> keys;
                if (!_keysById.TryGetValue(id, out keys))
                    _keysById[id] = keys = new HashSet<object>();
                keys.Add(key);
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        internal void Remove(Identity id, object key)
        {
            DebugContract.Requires(id);

            _sync.EnterWriteLock();
            try
            {
                // Si la valeur est nulle, on supprime toutes les références à l'ID
                if (key == null)
                {
                    HashSet<object> keys;
                    if (_keysById.TryGetValue(id, out keys))
                    {
                        foreach (var v in keys)
                        {
                            HashSet<Identity> list;
                            if (_index.TryGetValue(v, out list))
                            {
                                if (list.Remove(id) && list.Count == 0)
                                    _index.Remove(v);
                            }
                        }
                        _keysById.Remove(id);
                    }
                }
                else
                {
                    HashSet<Identity> list;
                    if (_index.TryGetValue(key, out list))
                    {
                        if (list.Remove(id) && list.Count == 0)
                            _index.Remove(key);
                    }
                    HashSet<object> keys;
                    if (_keysById.TryGetValue(id, out keys))
                    {
                        if (keys.Remove(key) && keys.Count == 0)
                            _keysById.Remove(id);
                    }
                }
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        private ISession EnsuresRunInSession()
        {
            if (Session.Current != null)
                return null;

            return (_graph.Target as IHyperGraph).DomainModel.Store.BeginSession(new SessionConfiguration
                                                                                        {
                                                                                                Readonly = true
                                                                                        });
        }
    }
}