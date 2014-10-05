using Hyperstore.Modeling.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Scopes
{
    class ScopeControler<TDomain> : IScopeManager<TDomain> where TDomain : class, IDomainModel
    {
        private readonly ThreadSafeLazyRef<ImmutableDictionary<string, Entry<TDomain>>> _scopes = new ThreadSafeLazyRef<ImmutableDictionary<string, Entry<TDomain>>>(() =>  ImmutableDictionary<string, Entry<TDomain>>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase));
        private HashSet<int> _activeSessions = new HashSet<int>();

        private int _lastSessionId;

        public ScopeControler(IHyperstore store)
        {
            Store = store;
        }

        public IHyperstore Store { get; private set; }

        void IScopeManager<TDomain>.OnSessionCreated(ISession session, int sessionId)
        {
            var id = session != null ? session.SessionId : sessionId; // test
            lock (_activeSessions)
            {
                _activeSessions.Add(id);
            }
            Interlocked.Exchange(ref _lastSessionId, id);
            if (session != null)
                session.Completing += SessionCompleting;
        }

        void SessionCompleting(object sender, SessionCompletingEventArgs e)
        {
            OnSessionCompleted(e.Session.SessionId);
        }

        internal void OnSessionCompleted(int sessionId)
        {
            lock (_activeSessions)
            {
                _activeSessions.Remove(sessionId);
            }

            Purge(sessionId);
        }

        private void Purge(int sessionId)
        {
            _scopes.ExchangeValue(scopes =>
            {
                foreach (var entry in scopes.Values)
                {
                    var name = entry.Domain.Name;
                    var e = entry.Purge(sessionId);
                    if (e == null)
                        scopes = scopes.Remove(name);
                    else if (e != entry)
                        scopes = scopes.SetItem(name, e);
                }
                return scopes;
            });
        }

        /// <summary>
        /// Add a new scope but not yet enable 
        /// </summary>
        /// <param name="domain"></param>
        void IScopeManager<TDomain>.RegisterScope(TDomain domain)
        {
            Entry<TDomain> entry;

            // Ajout dans la liste des scopes
            var name = domain.Name;
            _scopes.ExchangeValue(scopes =>
                {
                    if (scopes.TryGetValue(name, out entry))
                    {
                        return scopes.SetItem(name, entry.NewScope(domain));
                    }

                    entry = new Entry<TDomain>(domain);
                    return scopes.Add(name, entry);
                });
        }

        void IScopeManager<TDomain>.EnableScope(TDomain domain)
        {
            SetScopeEntry(domain, true);
        }

        private void SetScopeEntry(TDomain domain, bool load)
        {
            var name = domain.Name;
            // Rend le scope actif à partir de la prochaine session
            _scopes.ExchangeValue(scopes =>
            {
                Entry<TDomain> entry;
                if (scopes.TryGetValue(name, out entry))
                {
                    if (load)
                        entry = entry.SetStartSessionId(domain, _lastSessionId);
                    else
                    {
                        lock (_activeSessions)
                        {
                            entry = entry.SetEndSessionId(domain, _lastSessionId, _activeSessions);
                        }
                    }
                }

                System.Diagnostics.Debug.Assert(entry != null);
                return scopes.SetItem(name, entry);
            });
        }

        void IScopeManager<TDomain>.UnloadScope(TDomain domain)
        {
            SetScopeEntry(domain, false);
            Purge(0);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <returns>
        ///  A TDomain.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TDomain Get(string name, ISession session)
        {
            return ((IScopeManager<TDomain>)this).GetActiveScope(name, session != null ? session.SessionId : 0);
        }

        IEnumerable<TDomain> IScopeManager<TDomain>.GetScopes(ScopesSelector selector, int sessionId)
        {
            return _scopes.Value.Values.SelectMany(e => e.GetScopes(selector, sessionId));
        }

        TDomain IScopeManager<TDomain>.GetActiveScope(string name, int sessionId)
        {
            if (!_scopes.HasValue)
                return default(TDomain);

            Entry<TDomain> entry;
            _scopes.Value.TryGetValue(name, out entry);
            if (entry == null)
                return null;
            return entry.GetActiveScope(sessionId);
        }

        void IDisposable.Dispose()
        {
            _scopes.ExchangeValue(scopes =>
                {
                    foreach (var key in scopes.Keys.ToList())
                    {
                        scopes.SetItem(key, scopes[key].ForceUnload(_activeSessions));
                    }
                    return scopes;
                });
        }
    }
}
