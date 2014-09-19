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

using Hyperstore.Modeling.Platform;
using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Hyperstore.Modeling.HyperGraph.Index
{
    internal class MemoryIndexManager : IIndexManager
    {
        private readonly Dictionary<Identity, List<IndexDefinition>> _indexByMetaClass = new Dictionary<Identity, List<IndexDefinition>>();
        private readonly IConcurrentDictionary<string, IndexDefinition> _indexByNames;
        private readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();
        private IHyperGraph _graph;

        public IDomainModel DomainModel { get { return _graph.DomainModel; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="adapter">
        ///  The adapter.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public MemoryIndexManager(IHyperGraph adapter)
        {
            Contract.Requires(adapter, "adapter");

            _indexByNames = PlatformServices.Current.CreateConcurrentDictionary<string, IndexDefinition>();
            _graph = adapter;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            _graph = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the index.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///  Thrown when the requested operation is not supported.
        /// </exception>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="DuplicateIndexException">
        ///  Thrown when a Duplicate Index error condition occurs.
        /// </exception>
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
            Contract.Requires(metaclass, "metaclass");
            Contract.Requires(propertyNames.Length > 0, "propertyName");

            if (propertyNames.Length > 1)
                throw new NotSupportedException();

            var property = metaclass.GetProperty(propertyNames[0]);
            if (property == null)
                throw new Exception(string.Format(ExceptionMessages.PropertyNameNotValidForMetaclassFormat,  metaclass.Name));

            Conventions.CheckValidName(name);
            IndexDefinition def;
            _sync.EnterWriteLock();
            try
            {
                if (IndexExists(name))
                    throw new DuplicateIndexException(string.Format(ExceptionMessages.DuplicateIndexFormat, name));

                List<IndexDefinition> list = null;
                if (!_indexByMetaClass.TryGetValue(metaclass.Id, out list))
                {
                    list = new List<IndexDefinition>();
                    _indexByMetaClass.Add(metaclass.Id, list);
                }

                def = new IndexDefinition(_graph, name, metaclass, unique, propertyNames);
                list.Add(def);
                _indexByNames.TryAdd(name, def);
            }
            finally
            {
                _sync.ExitWriteLock();
            }

            // Build index
            if (_graph.DomainModel != null)
            {
                foreach (var mel in _graph.GetElements(metaclass))
                {
                    def.Index.Add(mel.Id, mel.GetPropertyValue(property).Value);
                }
            }

            return def.Index;
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
            Contract.RequiresNotEmpty(name, "name");

            _sync.EnterWriteLock();
            try
            {
                IndexDefinition def;
                if (!_indexByNames.TryGetValue(name, out def))
                    return;

                List<IndexDefinition> list = null;
                if (_indexByMetaClass.TryGetValue(def.MetaClass.Id, out list))
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        if (string.Compare(list[i].Index.Name, name, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            list.RemoveAt(i);
                            if (list.Count == 0)
                                _indexByMetaClass.Remove(def.MetaClass.Id);
                        }
                    }
                }

                _indexByNames.TryRemove(name, out def);
            }
            finally
            {
                _sync.ExitWriteLock();
            }
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
            Contract.RequiresNotEmpty(name, "name");

            var def = GetIndexDefinition(name);
            if (def != null)
                return new IndexWrapper(_graph.DomainModel.Store, def.Index);
            return null;
        }

        internal bool IndexExists(string name)
        {
            Contract.RequiresNotEmpty(name, "name");
            return GetIndexDefinition(name) != null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the index definitions fors in this collection.
        /// </summary>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the index definitions fors in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IndexDefinition> GetIndexDefinitionsFor(ISchemaElement metaclass)
        {
            Contract.Requires(metaclass, "metaclass");

            _sync.EnterReadLock();
            try
            {
                List<IndexDefinition> result = null;
                var sp = metaclass;
                while (sp != null)
                {
                    List<IndexDefinition> list;
                    if (_indexByMetaClass.TryGetValue(sp.Id, out list))
                    {
                        if (result == null)
                            result = new List<IndexDefinition>();
                        result.AddRange(list);
                    }
                    sp = sp.SuperClass;
                }
                return result;
            }
            finally
            {
                _sync.ExitReadLock();
            }
        }

        internal void AddToIndex(ISchemaElement metaclass, string indexName, Identity id, object key)
        {
            DebugContract.Requires(metaclass);
            DebugContract.RequiresNotEmpty(indexName);
            DebugContract.Requires(id);

            if (key == null)
                return;

            var def = GetIndexDefinition(indexName);
            if (def == null)
                return;

            def.Index.Add(id, key);
        }

        internal void RemoveFromIndex(ISchemaElement metaclass, string indexName, Identity id, object key)
        {
            DebugContract.Requires(metaclass);
            DebugContract.RequiresNotEmpty(indexName);
            DebugContract.Requires(id);

            var def = GetIndexDefinition(indexName);
            if (def == null)
                return;

            def.Index.Remove(id, key);
        }

        private IndexDefinition GetIndexDefinition(string indexName)
        {
            DebugContract.RequiresNotEmpty(indexName);

            IndexDefinition def;
            _indexByNames.TryGetValue(indexName, out def);
            return def;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the element evicted action.
        /// </summary>
        /// <param name="status">
        ///  The status.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void OnElementEvicted(EvictionProcess status, Identity id)
        {
            switch (status)
            {
                case EvictionProcess.Start:
                    _sync.EnterWriteLock();
                    break;
                case EvictionProcess.End:
                    _sync.ExitWriteLock();
                    break;
                case EvictionProcess.Eviction:
                    DebugContract.Requires(id);
                    foreach (var def in _indexByNames.Values)
                    {
                        def.Index.Remove(id, null);
                    }
                    break;
            }
        }
    }
}