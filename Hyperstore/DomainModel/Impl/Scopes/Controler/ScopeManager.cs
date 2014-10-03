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
 
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hyperstore.Modeling.Scopes
{
    internal class ScopeManager<T> : IScopeManager<T> where T : class,IDomainModel
    {
        // TODO gestion du dechargement d'un domaine
        class Entry
        {
            public readonly T DomainModel;
            public bool Enabled;

            public Entry(T domain, bool enabled)
            {
                this.DomainModel = domain;
                Enabled = enabled;
            }
        }

        private IImmutableList<T> _scopes;
        private readonly Dictionary<string, Entry> _scopeByNames = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new object();

        public IHyperstore Store { get; private set; }

        public ScopeManager(IHyperstore store)
        {
            Contract.Requires(store, "store");
            Store = store;
            _scopes = ImmutableList.Create<T>();
        }

        void IDisposable.Dispose()
        {
            lock (_sync)
            {
                foreach (var dm in _scopes)
                {
                    dm.Dispose();
                }
                _scopeByNames.Clear();
                _scopes = null;
            }
            Store = null;
        }

        void IScopeManager<T>.EnableScope(T domain)
        {
            lock (_sync)
            {
                Entry dm;
                if (_scopeByNames.TryGetValue(domain.Name, out dm))
                {
                    dm.Enabled = true;
                    if (!(domain is ISchema))
                        Interlocked.Exchange(ref _scopes, _scopes.Add(domain));
                }
            }
        }

        T IScopeManager<T>.GetActiveScope(string name)
        {
            lock (_sync)
            {
                Entry dm;
                if (_scopeByNames.TryGetValue(name, out dm) && dm.Enabled)
                    return dm.DomainModel;
                return null;
            }
        }

        void IScopeManager<T>.OnSessionCreated(ISession session)
        {
        }

        void IScopeManager<T>.RegisterScope(T domain)
        {
            lock (_sync)
            {
                // For schema, the domain is immediatly available
                var isSchema = domain is ISchema;
                _scopeByNames.Add(domain.Name, new Entry(domain, isSchema));
                if (isSchema)
                    Interlocked.Exchange(ref _scopes, _scopes.Add(domain));
            }
        }

        void IScopeManager<T>.UnloadScope(T domain)
        {
            lock (_sync)
            {
                Entry dm;
                if (_scopeByNames.TryGetValue(domain.Name, out dm))
                {
                    dm.DomainModel.Dispose();
                    _scopeByNames.Remove(domain.Name);
                    Interlocked.Exchange(ref _scopes, _scopes.Remove(domain));
                }
            }
        }


        IEnumerable<T> IScopeManager<T>.GetAllScopes()
        {
            return _scopes;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _scopes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _scopes.GetEnumerator();
        }
    }
}
