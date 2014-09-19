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
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Scopes
{
    /// <summary>
    ///     Controleur permettant de gérer les domaines et les extensions
    /// </summary>
    internal class ExtendedScopeManager<T> : IScopeManager<T> where T : class,IDomainModel
    {
        private readonly List<Guid> _activeSessions = new List<Guid>();
        private readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private List<ScopeStack<T>> _domainModels = new List<ScopeStack<T>>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="store">
        ///  The store.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ExtendedScopeManager(IHyperstore store)
        {
            Contract.Requires(store, "store");
            Store = store;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void IDisposable.Dispose()
        {
            var activeSessions = _activeSessions.ToList();
            if (Session.Current != null)
                activeSessions = activeSessions.Except(new[] { Session.Current.SessionId }).ToList();

            for (var i = _domainModels.Count - 1; i >= 0; i--)
            {
                var dm = _domainModels[i];
                dm.Unload(activeSessions);
            }

            // wait fin des session actives 
            while (true)
            {
                if (_domainModels.All(i => i.IsEmpty))
                    break;

                ThreadHelper.Sleep(200);
            }

            _domainModels.Clear();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Activation d'un domaine ou de son extension.
        /// </summary>
        /// <remarks>
        ///  L'activation d'un domaine s'effectue une fois qu'il a fini d'être initialisé. Il est alors
        ///  visible par tout le monde.
        /// </remarks>
        /// <param name="domain">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void ActivateScope(T domain)
        {
            DebugContract.Requires(domain != null);

            _sync.EnterWriteLock();
            try
            {
                var item = FindDomainStack(domain.Name);
                DebugContract.Assert(item != null);
                item.Activate(domain);
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unload domain extension.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void UnloadScope(T domainModel)
        {
            DebugContract.Requires(domainModel != null);
            ScopeStack<T> item;
            List<Guid> activeSessions;

            _sync.EnterWriteLock();
            try
            {
                item = FindDomainStack(domainModel.Name);
                if (item == null)
                    return;

                // TODO immutable
                activeSessions = new List<Guid>(_activeSessions);
            }
            finally
            {
                _sync.ExitWriteLock();
            }

            item.Unload(activeSessions, domainModel);
        }

        //   [DebuggerStepThrough]

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets domain model.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetActiveScope(string name)
        {
            DebugContract.RequiresNotEmpty(name, "name");
            return GetActiveScopes()
                    .FirstOrDefault(d => String.Compare(d.Name, name, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the domain model described by domainModel.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterScope(T domainModel)
        {
            DebugContract.Requires(domainModel != null);

            var domainExtension = domainModel as IScope;

            _sync.EnterWriteLock();
            try
            {
                // Recherche 
                var item = FindDomainStack(domainModel.Name);

                // Chargement normal
                if (domainExtension == null)
                {
                    if (item != null)
                    {
                        var throwException = true;
                        Guid? sessionId = null;
                        if (Session.Current != null)
                            sessionId = Session.Current.SessionId;

                        if (item.GetDomainModel(sessionId) == null)
                        {
                            // Le domaine n'a pas encore été déchargé. On va attendre un peu pour voir
                            // et on reessaye.
                            // TODO voir si il y a pas mieux à faire mais on ne peut pas réutiliser le domaine courant car il est dèjà 
                            // initialisé et ce n'est pas ce qu'on veut (On repart d'un nouveau domaine propre)
                            ThreadHelper.Sleep(200);
                            item = FindDomainStack(domainModel.Name);
                            throwException = item != null;
                        }

                        if (throwException)
                            throw new Exception(string.Format(ExceptionMessages.TryToLoadDuplicateDomainModelFormat, domainModel.Name));
                    }

                    var tmp = new List<ScopeStack<T>>(_domainModels)
                              {
                                      new ScopeStack<T>(domainModel.Name, new DomainInfo<T>(domainModel))
                              };
                    _domainModels = tmp;
                }
                else // Extension de domaine
                {
                    if (item == null) // TODO verif qu'il correspond au modèle à étendre
                        throw new Exception(ExceptionMessages.ExtendedDomainNotFound);

                    item.Load(domainModel, _activeSessions);
                }
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        private ScopeStack<T> FindDomainStack(string domainModelName)
        {
            // Ici on est sur d'être appelé dans une section protégé
            var item = _domainModels.FirstOrDefault(s => s.IsNameEquals(domainModelName));
            if (item != null && item.IsEmpty)
            {
                _domainModels.Remove(item);
                return null;
            }
            return item;
        }

        private IEnumerable<T> GetActiveScopes()
        {
            _sync.EnterReadLock();
            try
            {
                var list = new List<T>(8);
                var sessionId = Session.Current != null ? Session.Current.SessionId : Guid.Empty;
                var tmp = _domainModels;

                // Recherche les domaines actifs pour une session
                foreach (var swap in tmp)
                {
                    var item = swap.GetDomainModel(sessionId);
                    if (item != null)
                        list.Add(item);
                }
                return list;
            }
            finally
            {
                _sync.ExitReadLock();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Fournit la liste des domaines actuellement actifs.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the domain models in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetAllScopes()
        {
            _sync.EnterReadLock();
            try
            {
                var list = new List<T>(8); // TODO a optimiser (préparer la liste)
                var sessionId = Session.Current != null ? Session.Current.SessionId : Guid.Empty;
                var tmp = _domainModels;

                // Recherche les domaines actifs pour une session
                foreach (var swap in tmp)
                {
                    foreach (var dm in swap)
                    {
                        var item = dm.GetDomainModel(sessionId);
                        if (item != null)
                            list.Add(item);
                    }
                }
                return list;
            }
            finally
            {
                _sync.ExitReadLock();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the session created action.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void OnSessionCreated(ISession session)
        {
            _sync.EnterWriteLock();
            try
            {
                session.Store.Trace.WriteTrace(TraceCategory.DomainControler, "Register session {0}", session.SessionId);
                _activeSessions.Add(session.SessionId);
            }
            finally
            {
                _sync.ExitWriteLock();
            }

            session.Completing += OnSessionCompleted;
        }

        private void OnSessionCompleted(object sender, SessionCompletingEventArgs e)
        {
            _sync.EnterWriteLock();

            try
            {
                _activeSessions.Remove(e.Session.SessionId);
                foreach (var swap in _domainModels)
                {
                    swap.OnSessionCompleted(e.Session.SessionId);
                }

                _domainModels.RemoveAll(m => m.IsEmpty);
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        /// <summary>
        ///     A stack contains a domain and all its extensions. Extensions are stacked in a linked list because it's possible to
        ///     unload
        ///     an extension in any position.
        /// </summary>
        private class ScopeStack<TElement> : IEnumerable<IDomainInfos<TElement>> where TElement : class,IDomainModel
        {
            /// <summary>
            ///     Domain (first position) and its extensions
            /// </summary>
            private readonly LinkedList<IDomainInfos<TElement>> _list;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="name">
            ///  Domain name - Extensions shares the domain name.
            /// </param>
            /// <param name="domainInfos">
            ///  The domain infos.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public ScopeStack(string name, DomainInfo<TElement> domainInfos)
            {
                Name = name;
                _list = new LinkedList<IDomainInfos<TElement>>();
                _list.AddFirst(domainInfos);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Domain name - Extensions shares the domain name.
            /// </summary>
            /// <value>
            ///  The name.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public string Name { get; private set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets a value indicating whether this instance is empty.
            /// </summary>
            /// <value>
            ///  true if this instance is empty, false if not.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public bool IsEmpty
            {
                get { return _list.Count == 0; }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Query if 'name' is name equals.
            /// </summary>
            /// <param name="name">
            ///  Domain name - Extensions shares the domain name.
            /// </param>
            /// <returns>
            ///  true if name equals, false if not.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public bool IsNameEquals(string name)
            {
                DebugContract.RequiresNotEmpty(name);
                return String.Compare(Name, name, StringComparison.OrdinalIgnoreCase) == 0;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Push a domain or an extension on the stack.
            /// </summary>
            /// <param name="extension">
            ///  The extension.
            /// </param>
            /// <param name="activeSessions">
            ///  The active sessions.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void Load(TElement extension, List<Guid> activeSessions)
            {
                DebugContract.Requires(extension);
                DebugContract.Requires(activeSessions);
                extension.Store.Trace.WriteTrace(TraceCategory.DomainControler, "*** Load extension for {1} with active sessions {0}", String.Join(",", activeSessions), extension.Name);

                // TODO gestion si l'extension est en train d'être déchargée
                var item = new ExtensionInfo<TElement>(extension, new List<Guid>(activeSessions));
                if (_list.Any(i => i.IsExtensionNameExists(extension.ExtensionName)))
                    throw new Exception("Duplicate extension name " + extension.ExtensionName);

                // Tjs sur le haut
                _list.AddLast(item);

                extension.Store.Trace.WriteTrace(TraceCategory.DomainControler, " - OK");
            }

            internal void Activate(TElement domain)
            {
                foreach (var item in _list)
                {
                    item.Activate(domain);
                }
            }

            internal TElement GetDomainModel(Guid? sessionId)
            {
                var item = _list.Last;
                while (item != null)
                {
                    var dm = item.Value.GetDomainModel(sessionId);
                    if (dm != null)
                        return dm;
                    item = item.Previous;
                }
                return null;
            }

            internal void Unload(List<Guid> activeSessions, TElement domainModel = null)
            {
                var item = _list.Last;
                while (item != null)
                {
                    var prv = item.Previous;
                    if (item.Value.Unload(activeSessions, domainModel))
                        _list.Remove(item);
                    item = prv;
                }
            }

            internal void OnSessionCompleted(Guid sessionId)
            {
                var item = _list.Last;
                while (item != null)
                {
                    var prv = item.Previous;
                    if (item.Value.OnSessionCompleted(sessionId))
                        _list.Remove(item);
                    item = prv;
                }
            }

            public IEnumerator<IDomainInfos<TElement>> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store
        {
            get;
            private set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerator<T> GetEnumerator()
        {
            return GetActiveScopes().GetEnumerator();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetActiveScopes().GetEnumerator();
        }
    }
}