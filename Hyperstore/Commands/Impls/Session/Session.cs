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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Utils;
using Hyperstore.Modeling.Validations;

#endregion

#if NETFX_CORE

#endif

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A session.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISession"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISessionContext"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISessionInternal"/>
    ///-------------------------------------------------------------------------------------------------
    public class Session : ISession, ISessionContext, ISessionInternal
    {
        /// <summary>
        ///     Gestion des numéros de session.
        ///     Chaque session a un identifiant (SessionIndex) unique qui référence un contexte dans un dictionnaire
        ///     Pour éviter que le dictionnaire grossisse exagérément, on va optimiser la gestion des index en réutilisant
        ///     les index des sessions terminées.
        ///     On conserve la liste des index utilisés dans un tableau de bit. Si le bit correspondant à un index
        ///     est à 1, l'index est utilisé et ne peut pas être affecté à une nouvelle session.
        /// </summary>
        private static readonly SessionIndexProvider s_sessionSequences = new SessionIndexProvider();

        // Identifiant de session (stocké au niveau du thread)
        // Doit être initialisé pour chaque thread soit par la création d'une session, soit par using(new MultiThreadSession)
        [ThreadStatic]
        private static ushort? _sessionIndex;
        private readonly IHyperstoreTrace _trace;
        private ITransactionScope _scope; // scope est lui aussi lié au thread

        /// <summary>
        ///     Constructeur interne. Il est appelé par le store quand il crée la session
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="cfg">The CFG.</param>
        internal Session(IHyperstore store, SessionConfiguration cfg)
        {
            DebugContract.Requires(store, "store");

            if (SessionIndex == null)
                SessionIndex = s_sessionSequences.GetFirstFreeValue();

            _trace = store.Trace;

            var ctx = SessionDataContext;
            if (ctx == null)
            {
                // _statSessionCount.Incr();

                // Nouvelle session
                ctx = new SessionDataContext
                      {
                          TrackingData = new SessionTrackingData(this),
                          SessionIsolationLevel = cfg.IsolationLevel,
                          Locks = new List<ILockInfo>(),
                          ReadOnly = cfg.Readonly,
                          ReadOnlyStatus = cfg.Readonly,
                          Current = this,
                          OriginStoreId = store.Id,
                          Mode = cfg.Mode,
                          SessionId = cfg.SessionId ?? Guid.NewGuid(),
                          Store = store,
                          CancellationToken = cfg.CancellationToken,
                          Enlistment = new List<ISessionEnlistmentNotification>(),
                          SessionInfos = new Stack<SessionLocalInfo>(),
                          TopLevelSession=this
                      };

                SessionDataContext = ctx;

#if !TXSCOPE
                _scope = new HyperstoreTransactionScope(this, cfg.IsolationLevel, cfg.SessionTimeout);
#else
                _scope = new TransactionScopeWrapper(cfg.IsolationLevel, cfg.SessionTimeout);
#endif
            }
            else if (ctx.SessionInfos.Count == 0)
                throw new Exception(ExceptionMessages.CannotCreateNestedSessionInDisposingSession);

            ctx.SessionInfos.Push(new SessionLocalInfo
                                  {
                                      DefaultDomainModel = cfg.DefaultDomainModel,
                                      Mode = cfg.Mode | Mode
                                  });
        }

        internal static ushort? SessionIndex
        {
            get { return _sessionIndex; }
            set
            {
                DebugContract.Requires(value != null);
                _sessionIndex = value;
            }
        }

        //#if NETFX_CORE

        //        internal static ushort? SessionIndex
        //        {
        //            get
        //            {
        //                var ctx = SynchronizationContext.Current as HyperstoreSynchronizationContext;
        //                if (ctx == null)
        //                {
        //                    return null;
        //                }
        //                return ctx.SessionIndex;
        //            }
        //            set
        //            {
        //                if (value == null)
        //                {
        //                    var ctx = SynchronizationContext.Current as HyperstoreSynchronizationContext;
        //                    _sessionSequences.ReleaseValue(SessionIndex.Value);
        //                    SynchronizationContext.SetSynchronizationContext(ctx.OldContext);
        //                }
        //                else
        //                {
        //                    new HyperstoreSynchronizationContext(value.Value, SynchronizationContext.Current);
        //                }
        //            }
        //        }
        //#else
        //        private static readonly string SessionIndexKey = Guid.NewGuid().ToString("N");
        //  TODO : A remettre quand TOUS les frameworks supporteront le LogicalGetData (Penser à modifier le TransactionScopeAsyncFlowOption dans le TransactionScopeWrapper)
        //        internal static ushort? SessionIndex
        //        {
        //            get 
        //            {
        //                return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(SessionIndexKey) as Nullable<ushort>;
        //            }
        //            set 
        //            {
        //                if (value == null)
        //                {
        //                    _sessionSequences.ReleaseValue(SessionIndex.Value);
        //                }
        //                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(SessionIndexKey, value);
        //            }
        //        }
        //#endif

        private static SessionDataContext SessionDataContext
        {
            get
            {
                var ix = SessionIndex;
                if (ix == null)
                    return null;

                SessionDataContext ctx;
                SessionContexts.TryGetValue(ix.Value, out ctx);
                return ctx;
            }
            set
            {
                var ix = SessionIndex;
                if (ix != null)
                    SessionContexts[ix.Value] = value;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current.
        /// </summary>
        /// <value>
        ///  The current.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISession Current
        {
            get
            {
                var ctx = SessionDataContext;
                return ctx != null ? ctx.Current : null;
            }
        }

        internal List<ISessionEnlistmentNotification> Enlistment
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null)
                    return null;
                return ctx.Enlistment;
            }
        }
        /// <summary>
        ///     Liste des éléments impactés par les commandes lors de la session. Si plusieurs commandes opérent sur un même
        ///     élément, il
        ///     ne sera répertorié qu'une fois.
        /// </summary>
        /// <value>
        ///     The involved elements.
        /// </value>
        private IEnumerable<IModelElement> InvolvedElements
        {
            get
            {
                var trackingData = SessionDataContext.TrackingData;
                return trackingData.InvolvedModelElements;
            }
        }
        //public IEnumerable<DiagnosticMessage> Messages
        //{
        //    get { return SessionDataContext.MessageList.Messages; }
        //}

        //public bool HasErrors
        //{
        //    get { return SessionDataContext.MessageList.HasErrors; }
        //}

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Session terminated event.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler<SessionCompletingEventArgs> Completing;

        void ISessionInternal.PushExecutionScope()
        {
            var ctx = SessionDataContext;
            ctx.CommandExecutionScopeLevel++;
        }

        void ISessionInternal.PopExecutionScope()
        {
            var ctx = SessionDataContext;
            ctx.CommandExecutionScopeLevel--;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store
        {
            get { return SessionDataContext.Store; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default domain model.
        /// </summary>
        /// <value>
        ///  The default domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DefaultDomainModel
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null || ctx.Depth == 0)
                    return null;
                return ctx.SessionInfos.Peek()
                        .DefaultDomainModel;
            }
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the top level session.
        /// </summary>
        /// <value>
        ///  The top level session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISession TopLevelSession
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null || ctx.Depth == 0)
                    return null;
                return ctx.TopLevelSession;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///  true if this instance is read only, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsReadOnly
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null)
                    return true;
                return ctx.ReadOnlyStatus;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the origin store.
        /// </summary>
        /// <value>
        ///  The identifier of the origin store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid OriginStoreId
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null)
                    return Guid.Empty;
                return ctx.OriginStoreId;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets information describing the tracking.
        /// </summary>
        /// <value>
        ///  Information describing the tracking.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISessionTrackingData TrackingData
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null)
                    return null;
                return ctx.TrackingData;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the cancellation token.
        /// </summary>
        /// <value>
        ///  The cancellation token.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public CancellationToken CancellationToken
        {
            get
            {
                var ctx = SessionDataContext;
                return ctx.CancellationToken;
            }
        }

        ICollection<ILockInfo> ISession.Locks
        {
            get
            {
                var ctx = SessionDataContext;
                return ctx.Locks;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the isolation level.
        /// </summary>
        /// <value>
        ///  The isolation level.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionIsolationLevel SessionIsolationLevel
        {
            get
            {
                var ctx = SessionDataContext;
                return ctx.SessionIsolationLevel;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is nested.
        /// </summary>
        /// <value>
        ///  true if this instance is nested, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsNested
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null || ctx.Depth == 0)
                    return false;
                return ctx.Depth > 1;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this session is disposing.
        /// </summary>
        /// <value>
        ///  <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsDisposing
        {
            get
            {
                var ctx = SessionDataContext;
                return ctx.Disposing;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is aborted.
        /// </summary>
        /// <value>
        ///  true if this instance is aborted, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsAborted
        {
            get
            {
                var ctx = SessionDataContext;
                return ctx.Aborted;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Processes the command.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="SessionException">
        ///  Thrown when a Session error condition occurs.
        /// </exception>
        /// <param name="commands">
        ///  A variable-length parameters list containing command.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IExecutionResult Execute(params IDomainCommand[] commands)
        {
            if (commands == null || commands.Length == 0)
                return ExecutionResult.Empty;

            var domainCommands = commands.Where(cmd => cmd != null).ToList();
            if (domainCommands.Count == 0)
                return ExecutionResult.Empty;

            var result = new ExecutionResult();
            var commandsByDomains = from cmd in domainCommands group cmd by cmd.DomainModel.Name;

            ((ISessionInternal)this).PushExecutionScope();

            try
            {
                foreach (var cd in commandsByDomains)
                {
                    var dm = Store.GetDomainModel(cd.Key);
                    if (dm == null)
                        dm = Store.GetSchema(cd.Key);
                    if (dm == null)
                        throw new Exception();

                    if (dm is IUpdatableDomainModel)
                    {
                        var messages = ((IUpdatableDomainModel)dm).Commands.ProcessCommands(cd.ToArray());
                        result.AddMessages(messages);
                    }
                }
            }
            catch (SessionException)
            {
                // On force le rollback sur la session car l'appel courant a pu être intègré dans une session 
                // englobante          
                ((ISessionInternal)this).RejectChanges();
            }
            catch (Exception ex) // Si bug ds Hyperstore ;-)
            {
                result.AddMessage(new DiagnosticMessage(MessageType.Error, "Command aborted", "Fatal error", ex: ex));
                // On force le rollback sur la session car l'appel courant a pu être intègré dans une session 
                // englobante          
                ((ISessionInternal)this).RejectChanges();
            }
            finally
            {
                ((ISessionInternal)this).PopExecutionScope();
                // Sinon si on est dans une session englobante, il faut générer directement l'exception.
                if (result.HasErrors && (((ISessionInternal)this).Mode & SessionMode.SilentMode) != SessionMode.SilentMode)
                    throw new SessionException(result.Messages);
            }
            return result;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Acquires a lock for a property.
        /// </summary>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDisposable AcquireLock(LockType mode, Identity id, string propertyName = null)
        {
            Contract.Requires(id, "id");
            var ressource = id;
            if (!String.IsNullOrWhiteSpace(propertyName))
                ressource = id.CreateAttributeIdentity(propertyName);
            return AcquireLock(mode, ressource.ToString());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Acquires a lock.
        /// </summary>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        /// <param name="ressource">
        ///  The ressource.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDisposable AcquireLock(LockType mode, object ressource)
        {
            Contract.Requires(ressource, "ressource");

            return Store.LockManager.AcquireLock(this, ressource, mode);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enlists the specified transaction.
        /// </summary>
        /// <param name="transaction">
        ///  The transaction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Enlist(ITransaction transaction)
        {
            if (_scope != null)
                _scope.Enlist(transaction);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets context information.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <returns>
        ///  The context information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetContextInfo<T>(string key)
        {
            Contract.RequiresNotEmpty(key, "key");

            object obj;
            if (SessionDataContext.Infos.TryGetValue(key, out obj))
                return (T)obj;
            return default(T);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets context information.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetContextInfo(string key, object value)
        {
            Contract.RequiresNotEmpty(key, "key");

            if (value == null)
            {
                if (SessionDataContext.Infos.ContainsKey(key))
                    SessionDataContext.Infos.Remove(key);
            }
            else
                SessionDataContext.Infos[key] = value;
        }

        void ISessionInternal.SetOriginStoreId(Guid id)
        {
            var ctx = SessionDataContext;
            if (ctx == null || ctx.Depth == 0)
                return;
            ctx.OriginStoreId = id;
        }

        void ISession.SetMode(SessionMode mode)
        {
            var ctx = SessionDataContext;
            if (ctx == null || ctx.Depth == 0)
                return;

            ctx.SessionInfos.Peek().Mode |= mode;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the mode.
        /// </summary>
        /// <value>
        ///  The mode.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionMode Mode
        {
            get
            {
                var ctx = SessionDataContext;
                if (ctx == null || ctx.Depth == 0)
                    return SessionMode.Normal;

                return ctx.SessionInfos.Peek().Mode;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Suppression de la session. C'est dans cette méthode que seront envoyés les notifications, que
        ///  s'effectueront les validations si la transaction est une transaction racine.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Accepts the changes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void AcceptChanges()
        {
            var state = SessionDataContext.SessionInfos.Peek();
            state.Committed = true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Rejects the session (Abort).
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void RejectChanges()
        {
            SessionDataContext.Aborted = true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an event to the session events list.
        /// </summary>
        /// <exception cref="ReadOnlyException">
        ///  Thrown when a Read Only error condition occurs.
        /// </exception>
        /// <param name="event">
        ///  The event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void AddEvent(IEvent @event)
        {
            Contract.Requires(@event, "@event");

            if (SessionDataContext.ReadOnly)
                throw new ReadOnlyException("Read only session can not be modified");

            @event.IsTopLevelEvent = IsInTopLevelCommandScope();
            SessionDataContext.Events.Add(@event);
            SessionDataContext.TrackingData.OnEvent(@event);

            foreach (var notifier in GetNotifiers())
            {
                notifier.NotifyEvent(this, SessionContext, @event);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the events.
        /// </summary>
        /// <value>
        ///  The events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IEvent> Events
        {
            get { return SessionDataContext.Events; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the session.
        /// </summary>
        /// <value>
        ///  The identifier of the session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid SessionId
        {
            get { return SessionDataContext.SessionId; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session context.
        /// </summary>
        /// <value>
        ///  The session context.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISessionContext SessionContext
        {
            get { return this; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Logs the specified message.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Log(DiagnosticMessage message)
        {
            Contract.Requires(message, "message");
            SessionDataContext.MessageList.AddMessage(message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the result.
        /// </summary>
        /// <value>
        ///  The result.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IExecutionResult Result
        {
            get { return SessionDataContext.MessageList; }
        }

        internal static void ResetSessionIndex(bool releaseIndex)
        {
            if (releaseIndex)
                s_sessionSequences.ReleaseValue(SessionIndex.Value);
            _sessionIndex = null;
        }

        private bool IsInTopLevelCommandScope()
        {
            var ctx = SessionDataContext;
            return ctx.CommandExecutionScopeLevel == 1;
        }

        internal Dictionary<string, object> GetInfos()
        {
            return SessionDataContext.Infos;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Suppression de la session. C'est dans cette méthode que seront envoyés les notifications, que
        ///  s'effectueront les validations si la transaction est une transaction racine.
        /// </summary>
        /// <param name="disposing">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void Dispose(bool disposing)
        {
            var ctx = SessionDataContext;
            if (ctx == null)
                return;

            var currentMode = Mode;
            var currentInfo = ctx.SessionInfos.Pop();

            // Si la transaction n'a pas été validée, toutes les transactions englobantes deviennent invalides
            if (!currentInfo.Committed && !ctx.ReadOnly || ctx.CancellationToken.IsCancellationRequested)
                ctx.Aborted = true;

            // Top transaction
            if (ctx.Depth == 0)
            {
                using (CodeMarker.MarkBlock("Session.Dispose"))
                {
                    var notifiers = GetNotifiers();

                    var messages = CompleteTopLevelTransaction(notifiers, ctx, currentInfo);

                    if (!ctx.CancellationToken.IsCancellationRequested)
                        NotifyDiagnosticMessages(disposing, currentMode, notifiers, messages);
                }
            }
        }

        private void NotifyDiagnosticMessages(bool disposing, SessionMode currentMode, IEnumerable<IEventNotifier> notifiers, ExecutionResult messages)
        {
            try
            {
                // Enfin notification des erreurs
                // Cette partie ne doit pas planter toute erreur sera ignorée
                using (CodeMarker.MarkBlock("Session.Notifications"))
                {
                    foreach (var notifier in notifiers)
                    {
                        notifier.NotifyMessages(messages);
                    }
                }
            }
            catch (Exception ex)
            {
                messages.AddMessage(new DiagnosticMessage(MessageType.Error, ex.Message, "Session", SessionDataContext.InValidationProcess, null, ex));
                _trace.WriteTrace(TraceCategory.Session, ExceptionMessages.Trace_FatalErrorInEventsNotificationProcessFormat, ex.Message);
            }
            finally
            {
                if (disposing && messages.HasErrors && (currentMode & SessionMode.SilentMode) != SessionMode.SilentMode)
                    throw new SessionException(messages.Messages);
            }
        }

        private ExecutionResult CompleteTopLevelTransaction(IEnumerable<IEventNotifier> notifiers, SessionDataContext ctx, SessionLocalInfo currentInfo)
        {
            // Sauvegarde des références vers les objets qui sont utilisés aprés que les données de la session auront été supprimées
            ExecutionResult messages = null;

            // Il ne peut pas y avoir d'erreur dans cette partie de code
            try
            {
                ctx.Disposing = true;

                // Si la session était en lecture seule, on simplifie les traitements
                // Pas de validation
                if (!ctx.ReadOnly)
                {
                    messages = ExecuteConstraints(ctx, currentInfo);
                }

                // Contexte en lecture seule de la session mais tjs dans le scope
                // Envoi des events même en read-only pour s'assurer que le OnSessionCompleted soit bien notifié
                if (!ctx.CancellationToken.IsCancellationRequested)
                {
                    using (CodeMarker.MarkBlock("Session.OnSessionCompleted"))
                    {
                        OnSessionCompleted(currentInfo, notifiers);
                    }
                }

                // Si tout va bien, on commite
                if (!IsAborted && _scope != null && (messages == null || !messages.HasErrors))
                    _scope.Complete();
            }
            catch (Exception ex)
            {
                Log(new DiagnosticMessage(MessageType.Error, ex.Message, ExceptionMessages.Diagnostic_ApplicationError, SessionDataContext.InValidationProcess, null, ex));
            }
            finally
            {
                DisposeSession(ctx);
            }

            return messages ?? ExecutionResult.Empty;
        }

        private ExecutionResult ExecuteConstraints(SessionDataContext ctx, SessionLocalInfo currentInfo)
        {
            var messages = ctx.MessageList;

            // Notification des événements.
            // La notification se fait encore dans le scope actif

            // A partir d'ici, les événements (issues des commandes) NE SONT PLUS pris en compte car si une nouvelle commande est émise
            // il faut pouvoir :
            //  1 - exécuter les contraintes sur les nouvelles modifications.
            //  2 - renvoyer les nouveaux évents.
            // Ce qui n'est pas possible à partir d'ici puisqu'on est dans le processus gérant ces cas.
            //
            // Si on veut générer d'autres events, il faut s'abonner dans un autre thread (pour pouvoir créer une nouvelle session)
            ctx.ReadOnly = true;
            var hasInvolvedElements = false;
            using (CodeMarker.MarkBlock("Session.PrepareInvolvedElements"))
            {
                // Récupération des éléments impactés au cours de la session
                // Ne pas le faire lors des chargements des metadonnées  car :
                //  1 - Il n'y a pas de contraintes sur les metadonnées
                //  2 - En cas de chargement d'une extension, les métadonnées ne sont pas encore accessibles et cela fait planter le code.
                hasInvolvedElements = SessionDataContext.TrackingData.PrepareModelElements(IsAborted || (currentInfo.Mode & SessionMode.SkipConstraints) == SessionMode.SkipConstraints);
            }

            // Validation implicite sur les éléments modifiés au cours de la session
            // Ce code ne doit pas se faire lorsqu'on charge les metadonnées
            if (hasInvolvedElements)
            {
                try
                {
                    // Vérification des contraintes implicites
                    using (CodeMarker.MarkBlock("Session.CheckConstraints"))
                    {
                        CheckConstraints(InvolvedElements);
                    }
                    // Une contrainte peut entrainer un rollback de la transaction
                    // si il retourne une erreur dans result.
                    ctx.Aborted = ctx.MessageList != null && ctx.MessageList.HasErrors;
                }
                catch (Exception ex)
                {
                    ctx.Aborted = true;
                    var exception = ex;
                    if (exception is AggregateException)
                        exception = ((AggregateException)ex).InnerException;
                    Log(new DiagnosticMessage(MessageType.Error, exception.Message, ExceptionMessages.Diagnostic_ConstraintsProcessError, SessionDataContext.InValidationProcess, null, exception));
                }
            }
            return messages;
        }

        private void DisposeSession(SessionDataContext ctx)
        {

            try
            {
                if (_scope != null)
                {
                    using (CodeMarker.MarkBlock("Session.ScopeDispose"))
                    {
                        _scope.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = ex;
                if (exception is AggregateException)
                    exception = ((AggregateException)ex).InnerException;
                Log(new DiagnosticMessage(MessageType.Error, exception.Message, "Session", SessionDataContext.InValidationProcess, null, exception));
            }
            _scope = null;

            try
            {
                // Libération des locks                    
                Store.LockManager.ReleaseLocks(ctx.Locks, ctx.Aborted);
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionMessages.CriticalErrorMaybeAwaitInSession, ex);
            }
         
            // Suppression de la session courante. Il n'est plus possible de faire référence à la session
            ctx.Current = null;
            SessionDataContext = null;
            ResetSessionIndex(true);
        }

        private List<IEventNotifier> GetNotifiers()
        {
            var extensions = Store as IExtensionManager;
            if (extensions == null)
                throw new Exception("Store must implement IExtensionManager");

            var domainNotifiers = extensions.GetAllDomainModelIncludingExtensions()
                    .Where(domainModel => domainModel.Events is IEventNotifier)
                    .Select(domainmodel => domainmodel.Events as IEventNotifier);

            var schemaNotifiers = extensions.GetAllSchemaIncludingExtensions()
                    .Where(domainModel => domainModel.Events is IEventNotifier)
                    .Select(domainmodel => domainmodel.Events as IEventNotifier);

            return domainNotifiers.Union(schemaNotifiers).ToList();
        }

        // Vérification des contraintes implicites.
        // Ces contraintes ne portent que sur les éléments impactés lors de l'exécution de la session.
        private void CheckConstraints(IEnumerable<IModelElement> involvedElements)
        {
            try
            {
                SessionDataContext.InValidationProcess = true;
                var idx = SessionIndex;
                var threadId = ThreadHelper.CurrentThreadId;

                Parallel.ForEach(involvedElements, elem =>
                {
                    using (new MultiThreadedSession(idx.Value, threadId)) // Partage de session
                    {
                        var constraints = elem.SchemaInfo.Schema.Constraints as IImplicitDomainModelConstraints;
                        if (constraints != null)
                            constraints.ImplicitValidation(this, elem);
                    }
                });
            }
            finally
            {
                SessionDataContext.InValidationProcess = false;
            }
        }

        /// <summary>
        ///     Notification de la fin de la session
        /// </summary>
        private void OnSessionCompleted(SessionLocalInfo info, IEnumerable<IEventNotifier> notifiers)
        {
            DebugContract.Requires(notifiers!=null, "notifiers");

            try
            {
                // On fait une copie de la session car les événements peuvent 
                // être souscrits dans un autre thread
                var sessionContext = new SessionInformation(this, info, SessionDataContext.TrackingData);

                // Déclenchement des événements
                // D'abord évenement hard
                var tmp = Completing;
                if (tmp != null)
                    tmp(this, new SessionCompletingEventArgs(sessionContext));

                // Notifications via RX.
                foreach (var notifier in notifiers)
                {
                    notifier.NotifySessionCompleted(sessionContext, SessionContext);
                }
            }
            catch (Exception ex)
            {
                // Si une erreur survient dans une notification, on l'intercepte
                Log(new DiagnosticMessage(MessageType.Error, ex.Message, "SessionTerminated", SessionDataContext.InValidationProcess, null, ex));
            }
        }

        private class MultiThreadedSession : IDisposable
        {
            private readonly int _threadId;

            internal MultiThreadedSession(ushort? sessionIndex, int threadId)
            {
                DebugContract.Requires(sessionIndex > 0);
                SessionIndex = sessionIndex;
                _threadId = threadId;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
            ///  resources.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Dispose()
            {
                if (_threadId != ThreadHelper.CurrentThreadId)
                    ResetSessionIndex(false);
            }
        }

        #region Data thread

        // Les données de session sont partagés entre tous les threads afin de pouvoir partager une session sur plusieurs threads.
        // Ceci est possible en propageant le numéro de session entre les threads (SessionIndex)
        // Il est à noter que le TransactionScope est lié à un thread et ne peut pas être partagé, c'est pour cela que le partage de session
        // doit se faire en mode read only au niveau de la session.
        // 
        // Ce mécanisme est utilisé pour lancer les contraintes en // (voir CheckConstraints)
        //
        // Les données sont partagées en utilisant la méthode GetContextInfo<> de la session courante.
        //
        // Données de session partagées
        private static readonly ConcurrentDictionary<UInt16, SessionDataContext> SessionContexts = new ConcurrentDictionary<UInt16, SessionDataContext>();

        #endregion
    }
}