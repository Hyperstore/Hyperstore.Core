using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling.Scopes
{
    class Entry<T>  where T : class, IDomainModel
    {
        public T Domain { get; private set; }
        private int _startSessionId;
        private int _endSessionId;
        private ImmutableHashSet<int> _pendingSessions;
        private Entry<T> _next;

        public Entry(T domain, Entry<T> next=null)
        {
            this.Domain = domain;
            _next = next;
        }

        public Entry(Entry<T> entry, Entry<T> next=null, int start=0, int end=0, HashSet<int> pendings=null)
        {
            this._next = next ?? entry._next;
            _startSessionId = start != 0 ? start : entry._startSessionId;
            _endSessionId = end != 0 ? end : entry._endSessionId;
            Domain = entry.Domain;
            _pendingSessions = pendings != null ? pendings.ToImmutableHashSet() : entry._pendingSessions;
        }

        public Entry(Entry<T> entry, Entry<T> next, ImmutableHashSet<int> pendings) : this(entry, next)
        {
            this._pendingSessions = pendings;
        }

        public Entry<T> Purge(int sessionId)
        {
            var entry = this;
            var pendings = _pendingSessions;
            var next = _next;

            if (_pendingSessions != null)
            {
                pendings = _pendingSessions.Remove(sessionId);
                if (pendings.Count == 0)
                {
                    Domain.Dispose();
                    if (_next == null)
                        return null;
                }
            }

            if (_next != null)
            {
                next = _next.Purge(sessionId);
            }

            if (next != _next || pendings != _pendingSessions)
                entry = new Entry<T>(this, next, pendings);


            return entry;
        }

        public Entry<T> SetStartSessionId(T domain, int sessionId)
        {
            if (domain == Domain)
            {
                if (_endSessionId != 0)
                {
                    Debug.Assert(_startSessionId != 0); // Domain is being unloaded 
                }
                var entry = new Entry<T>(this, _endSessionId != 0 ? this : _next, sessionId + 1 ); // Ensure _startSessionId > 0
                return entry;
            }

            Debug.Assert(_next != null);

            return _next.SetStartSessionId(domain, sessionId);
        }

        internal Entry<T> SetEndSessionId(T domain, int sessionId, HashSet<int> activeSessions)
        {
            if (domain == Domain)
            {
                var entry = new Entry<T>(this, end:sessionId, pendings: activeSessions);
                return entry;
            }

            Debug.Assert(_next != null);

            return new Entry<T>(this, _next.SetEndSessionId(domain, sessionId, activeSessions));
        }

        internal T GetActiveScope(int sessionId)
        {
            if (IsActive(sessionId))
                return Domain;

            if (_next != null)
                return _next.GetActiveScope(sessionId);

            return null;
        }

        private bool IsActive(int sessionId)
        {
            return _startSessionId != 0
                        && ((sessionId == 0 && _endSessionId == 0)
                             || (_startSessionId <= sessionId && (_endSessionId == 0 || sessionId <= _endSessionId)));
        }

        internal Entry<T> NewScope(T domain)
        {
            var entry = new Entry<T>(domain, this);
            return entry;
        }

        internal IEnumerable<T> GetScopes(ScopesSelector selector, int sessionId)
        {
            var next = this;
            while (next != null)
            {
                if (selector == ScopesSelector.Loaded)
                {
                    if (_startSessionId != 0)
                        yield return Domain;
                }
                else if (IsActive(sessionId))
                {
                    yield return Domain;
                    yield break; // Only one active domain for a session
                }
                next = next._next;
            }
        }

        internal Entry<T> ForceUnload(HashSet<int> activeSessions)
        {
            return new Entry<T>(this, _next != null ? _next.ForceUnload(activeSessions) : null, pendings: activeSessions);
        }
    }
}
