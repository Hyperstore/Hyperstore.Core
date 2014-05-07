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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Events
{
    internal sealed class EventManager : IEventNotifier, IEventManager, IDomainService
    {
        #region SubjectFactory

        private class DefaultSubjectFactory : ISubjectFactory
        {
            private readonly IDependencyResolver _resolver;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="resolver">
            ///  The resolver.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public DefaultSubjectFactory(IDependencyResolver resolver)
            {
                DebugContract.Requires(resolver);
                _resolver = resolver;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Creates the subject.
            /// </summary>
            /// <typeparam name="T">
            ///  Generic type parameter.
            /// </typeparam>
            /// <returns>
            ///  The new subject.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public ISubjectWrapper<T> CreateSubject<T>()
            {
                return new Subject<T>(_resolver);
            }
        }

        #endregion

        #region Fields

        private readonly ISubjectWrapper<EventContext<ChangePropertyValueEvent>> _attributeChanged;

        private readonly ISubjectWrapper<EventContext<ChangePropertyValueEvent>> _attributeChanging;
        private readonly ISubjectWrapper<EventContext<RemovePropertyEvent>> _attributeRemoved;
        private readonly ISubjectWrapper<EventContext<RemovePropertyEvent>> _attributeRemoving;
        private readonly ISubjectWrapper<EventContext<IEvent>> _customEventRaising;
        private readonly ISubjectWrapper<EventContext<IEvent>> _customEvents;
        private readonly ISubjectWrapper<EventContext<AddEntityEvent>> _elementAdded;
        private readonly ISubjectWrapper<EventContext<AddEntityEvent>> _elementAdding;
        private readonly ISubjectWrapper<EventContext<RemoveEntityEvent>> _elementRemoved;
        private readonly ISubjectWrapper<EventContext<RemoveEntityEvent>> _elementRemoving;
        private readonly ISubjectWrapper<IExecutionResult> _messageOccurs;
        private readonly ISubjectWrapper<EventContext<AddSchemaEntityEvent>> _metadataAdded;
        private readonly ISubjectWrapper<EventContext<AddSchemaEntityEvent>> _metadataAdding;
        private readonly ISubjectWrapper<EventContext<AddRelationshipEvent>> _relationshipAdded;
        private readonly ISubjectWrapper<EventContext<AddRelationshipEvent>> _relationshipAdding;
        private readonly ISubjectWrapper<EventContext<AddSchemaRelationshipEvent>> _relationshipMetadataAdded;
        private readonly ISubjectWrapper<EventContext<AddSchemaRelationshipEvent>> _relationshipMetadataAdding;
        private readonly ISubjectWrapper<EventContext<RemoveRelationshipEvent>> _relationshipRemoved;
        private readonly ISubjectWrapper<EventContext<RemoveRelationshipEvent>> _relationshipRemoving;


        private readonly ISubjectWrapper<ISessionInformation> _sessionCompleted; 
        private readonly ISubjectWrapper<ISessionInformation> _sessionCompleting;
        private IDomainModel _domainModel;

        #endregion

        #region Properties

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the attribute changed.
        /// </summary>
        /// <value>
        ///  The attribute changed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<ChangePropertyValueEvent>> PropertyChanged
        {
            get { return _attributeChanged; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the attribute removed.
        /// </summary>
        /// <value>
        ///  The attribute removed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<RemovePropertyEvent>> PropertyRemoved
        {
            get { return _attributeRemoved; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the custom events.
        /// </summary>
        /// <value>
        ///  The custom events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<IEvent>> CustomEventRaised
        {
            get { return _customEvents; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element added.
        /// </summary>
        /// <value>
        ///  The element added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddEntityEvent>> ElementAdded
        {
            get { return _elementAdded; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element removed.
        /// </summary>
        /// <value>
        ///  The element removed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<RemoveEntityEvent>> ElementRemoved
        {
            get { return _elementRemoved; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity added.
        /// </summary>
        /// <value>
        ///  The schema entity added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddSchemaEntityEvent>> SchemaEntityAdded
        {
            get { return _metadataAdded; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship added.
        /// </summary>
        /// <value>
        ///  The schema relationship added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddSchemaRelationshipEvent>> SchemaRelationshipAdded
        {
            get { return _relationshipMetadataAdded; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship added.
        /// </summary>
        /// <value>
        ///  The relationship added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddRelationshipEvent>> RelationshipAdded
        {
            get { return _relationshipAdded; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship removed.
        /// </summary>
        /// <value>
        ///  The relationship removed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<RemoveRelationshipEvent>> RelationshipRemoved
        {
            get { return _relationshipRemoved; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property changing.
        /// </summary>
        /// <value>
        ///  The property changing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<ChangePropertyValueEvent>> PropertyChanging
        {
            get { return _attributeChanging; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property removing.
        /// </summary>
        /// <value>
        ///  The property removing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<RemovePropertyEvent>> PropertyRemoving
        {
            get { return _attributeRemoving; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the custom event raising.
        /// </summary>
        /// <value>
        ///  The custom event raising.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<IEvent>> CustomEventRaising
        {
            get { return _customEventRaising; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element adding.
        /// </summary>
        /// <value>
        ///  The element adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddEntityEvent>> ElementAdding
        {
            get { return _elementAdding; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element removing.
        /// </summary>
        /// <value>
        ///  The element removing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<RemoveEntityEvent>> ElementRemoving
        {
            get { return _elementRemoving; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity adding.
        /// </summary>
        /// <value>
        ///  The schema entity adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddSchemaEntityEvent>> SchemaEntityAdding
        {
            get { return _metadataAdding; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship adding.
        /// </summary>
        /// <value>
        ///  The schema relationship adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddSchemaRelationshipEvent>> SchemaRelationshipAdding
        {
            get { return _relationshipMetadataAdding; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship adding.
        /// </summary>
        /// <value>
        ///  The relationship adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<AddRelationshipEvent>> RelationshipAdding
        {
            get { return _relationshipAdding; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship removing.
        /// </summary>
        /// <value>
        ///  The relationship removing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<EventContext<RemoveRelationshipEvent>> RelationshipRemoving
        {
            get { return _relationshipRemoving; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session completed.
        /// </summary>
        /// <value>
        ///  The session completed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<ISessionInformation> SessionCompleted
        {
            get { return _sessionCompleted; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session completed.
        /// </summary>
        /// <value>
        ///  The session completed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<ISessionInformation> SessionCompleting
        {
            get { return _sessionCompleting; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the on errors.
        /// </summary>
        /// <value>
        ///  The on errors.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IObservable<IExecutionResult> OnErrors
        {
            get { return _messageOccurs; }
        }

        #endregion

        #region Constructors

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EventManager(IDependencyResolver resolver)
        {
            var factory = resolver.Resolve<ISubjectFactory>() ?? new DefaultSubjectFactory(resolver);

            _messageOccurs = factory.CreateSubject<IExecutionResult>();
            _sessionCompleted = factory.CreateSubject<ISessionInformation>();
            _sessionCompleting = factory.CreateSubject<ISessionInformation>();

            _attributeChanged = factory.CreateSubject<EventContext<ChangePropertyValueEvent>>();
            _attributeRemoved = factory.CreateSubject<EventContext<RemovePropertyEvent>>();
            _elementAdded = factory.CreateSubject<EventContext<AddEntityEvent>>();
            _elementRemoved = factory.CreateSubject<EventContext<RemoveEntityEvent>>();
            _relationshipAdded = factory.CreateSubject<EventContext<AddRelationshipEvent>>();
            _relationshipRemoved = factory.CreateSubject<EventContext<RemoveRelationshipEvent>>();
            _customEvents = factory.CreateSubject<EventContext<IEvent>>();
            _metadataAdded = factory.CreateSubject<EventContext<AddSchemaEntityEvent>>();
            _relationshipMetadataAdded = factory.CreateSubject<EventContext<AddSchemaRelationshipEvent>>();

            _attributeChanging = factory.CreateSubject<EventContext<ChangePropertyValueEvent>>();
            _attributeRemoving = factory.CreateSubject<EventContext<RemovePropertyEvent>>();
            _elementAdding = factory.CreateSubject<EventContext<AddEntityEvent>>();
            _elementRemoving = factory.CreateSubject<EventContext<RemoveEntityEvent>>();
            _relationshipAdding = factory.CreateSubject<EventContext<AddRelationshipEvent>>();
            _relationshipRemoving = factory.CreateSubject<EventContext<RemoveRelationshipEvent>>();
            _customEventRaising = factory.CreateSubject<EventContext<IEvent>>();
            _metadataAdding = factory.CreateSubject<EventContext<AddSchemaEntityEvent>>();
            _relationshipMetadataAdding = factory.CreateSubject<EventContext<AddSchemaRelationshipEvent>>();
        }

        #endregion

        #region Methods

        private readonly Dictionary<Identity, int> _attributedChangedObservers = new Dictionary<Identity, int>();
        private readonly ReaderWriterLockSlim _attributedChangedObserversSync = new ReaderWriterLockSlim();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initialisation du service avec le domaine associé. Cette méthode est appelée quand le service
        ///  est instancié par le domaine.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel, "domainModel");

            _domainModel = domainModel;
            //// Création d'un dispatcher
            //this._eventDispatcher = new EventDispatcher(domainModel.Store, domainModel, false);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers for attribute changed event.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public IDisposable RegisterForAttributeChangedEvent(IModelElement element)
        {
            if (element is IPropertyChangedNotifier)
            {
                DebugContract.Requires(element.Id);
                _attributedChangedObserversSync.EnterWriteLock();
                try
                {
                    if (_attributedChangedObservers.ContainsKey(element.Id))
                        _attributedChangedObservers[element.Id]++;
                    else
                        _attributedChangedObservers.Add(element.Id, 1);
                    return Disposables.ExecuteOnDispose(() => { _attributedChangedObservers[element.Id]--; if (_attributedChangedObservers[element.Id] == 0) _attributedChangedObservers.Remove(element.Id);});
                }
                finally
                {
                    _attributedChangedObserversSync.ExitWriteLock();
                }
            }
            return Disposables.Empty;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unregisters for attribute changed event.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void UnregisterForAttributeChangedEvent(IModelElement element)
        {
            _attributedChangedObserversSync.EnterWriteLock();
            try
            {
                var cx = --_attributedChangedObservers[element.Id];
                if (cx == 0)
                    _attributedChangedObservers.Remove(element.Id);
            }
            finally
            {
                _attributedChangedObserversSync.ExitWriteLock();
            }
        }

        void IEventNotifier.NotifyEvent(ISessionInformation session, ISessionContext log, IEvent ev)
        {
            DebugContract.Requires(session, "session");
            DebugContract.Requires(log, "log");
            DebugContract.Requires(ev, "ev");

            if (ev.DomainModel != _domainModel.Name && ev.ExtensionName != _domainModel.ExtensionName)
                return;

            try
            {
                if (ev is ChangePropertyValueEvent)
                    _attributeChanging.OnNext(new EventContext<ChangePropertyValueEvent>(session, (ChangePropertyValueEvent)ev));
                else if (ev is AddEntityEvent)
                    _elementAdding.OnNext(new EventContext<AddEntityEvent>(session, (AddEntityEvent)ev));
                else if (ev is AddRelationshipEvent)
                    _relationshipAdding.OnNext(new EventContext<AddRelationshipEvent>(session, (AddRelationshipEvent)ev));
                else if (ev is RemoveRelationshipEvent)
                    _relationshipRemoving.OnNext(new EventContext<RemoveRelationshipEvent>(session, (RemoveRelationshipEvent)ev));
                else if (ev is RemoveEntityEvent)
                    _elementRemoving.OnNext(new EventContext<RemoveEntityEvent>(session, (RemoveEntityEvent)ev));
                else if (ev is RemovePropertyEvent)
                    _attributeRemoving.OnNext(new EventContext<RemovePropertyEvent>(session, (RemovePropertyEvent)ev));
                else if (ev is AddSchemaEntityEvent)
                    _metadataAdding.OnNext(new EventContext<AddSchemaEntityEvent>(session, (AddSchemaEntityEvent)ev));
                else if (ev is AddSchemaRelationshipEvent)
                    _relationshipMetadataAdding.OnNext(new EventContext<AddSchemaRelationshipEvent>(session, (AddSchemaRelationshipEvent)ev));
                else
                    _customEventRaising.OnNext(new EventContext<IEvent>(session, ev));
            }
            catch (Exception ex)
            {
                NotifyEventError(log, ex);
            }
        }

        /// <summary>
        ///     Send events when the session end (correctly or not)
        /// </summary>
        /// <param name="session"></param>
        /// <param name="log"></param>
        void IEventNotifier.NotifySessionCompleted(ISessionInformation session, ISessionContext log)
        {
            DebugContract.Requires(session, "session");
            DebugContract.Requires(log, "log");

            if (!session.Events.Any(e => e.DomainModel == _domainModel.Name))
                return;

            try
            {
                _sessionCompleting.OnNext(session);
            }
            catch (Exception ex)
            {
                NotifyEventError(log, ex);
            }

            // Si la session s'est terminée anormalement, aucun autre événement n'est envoyé
            if (!session.IsAborted)
            {

                foreach (var ev in session.Events.Where(e => e.DomainModel == _domainModel.Name))
                {
                    // Utiliser plutot le eventbus
                    // this._eventDispatcher.HandleEvent(ev);
                    try
                    {
                        if (ev is AddEntityEvent)
                            _elementAdded.OnNext(new EventContext<AddEntityEvent>(session, (AddEntityEvent)ev));
                        else if (ev is AddRelationshipEvent)
                            _relationshipAdded.OnNext(new EventContext<AddRelationshipEvent>(session, (AddRelationshipEvent)ev));
                        else if (ev is RemoveRelationshipEvent)
                            _relationshipRemoved.OnNext(new EventContext<RemoveRelationshipEvent>(session, (RemoveRelationshipEvent)ev));
                        else if (ev is RemoveEntityEvent)
                            _elementRemoved.OnNext(new EventContext<RemoveEntityEvent>(session, (RemoveEntityEvent)ev));
                        else if (ev is RemovePropertyEvent)
                            _attributeRemoved.OnNext(new EventContext<RemovePropertyEvent>(session, (RemovePropertyEvent)ev));
                        else if (ev is ChangePropertyValueEvent)
                        {
                            var cmd = (ChangePropertyValueEvent)ev;
                            OnChangedAttributeEvent(session, log, cmd);
                        }
                        else if (ev is AddSchemaEntityEvent)
                            _metadataAdded.OnNext(new EventContext<AddSchemaEntityEvent>(session, (AddSchemaEntityEvent)ev));
                        else if (ev is AddSchemaRelationshipEvent)
                            _relationshipMetadataAdded.OnNext(new EventContext<AddSchemaRelationshipEvent>(session, (AddSchemaRelationshipEvent)ev));
                        else
                            _customEvents.OnNext(new EventContext<IEvent>(session, ev));
                    }
                    catch (Exception ex)
                    {
                        NotifyEventError(log, ex);
                    }
                }
            }

            try
            {
                _sessionCompleted.OnNext(session);
            }
            catch (Exception ex)
            {
                NotifyEventError(log, ex);
            }
        }

        void IEventNotifier.NotifyEnd()
        {
            try
            {
                _sessionCompleted.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _attributeChanged.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _attributeRemoved.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _elementAdded.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _elementRemoved.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _relationshipAdded.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _relationshipRemoved.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _customEvents.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _metadataAdded.OnCompleted();
            }
            catch
            {
            }
            try
            {
                _relationshipMetadataAdded.OnCompleted();
            }
            catch
            {
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send all validations message raises during the session.
        /// </summary>
        /// <param name="log">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void NotifyMessages(IExecutionResult log)
        {
            Contract.Requires(log, "log");
            _messageOccurs.OnNext(log);
        }

        /// <summary>
        ///     Une erreur est survenue pendant la notification d'un événement.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="ex"></param>
        private static void NotifyEventError(ISessionContext log, Exception ex)
        {
            Contract.Requires(log, "log");
            log.Log(new DiagnosticMessage(MessageType.Error, ex.Message, "Notifications", false, null, ex));
        }

        private void OnChangedAttributeEvent(ISessionInformation session, ISessionContext log, ChangePropertyValueEvent cmd)
        {
            DebugContract.Requires(session);
            DebugContract.Requires(log);
            DebugContract.Requires(cmd);

            if (!Equals(cmd.OldValue, cmd.Value))
            {
                var ctx = new EventContext<ChangePropertyValueEvent>(session, cmd);
                try
                {
                    _attributeChanged.OnNext(ctx);
                }
                catch (Exception ex)
                {
                    NotifyEventError(log, ex);
                }

                var list = new List<IModelElement>();
                _attributedChangedObserversSync.EnterReadLock();
                try
                {
                    foreach (var mel in session.TrackingData.InvolvedModelElements)
                    {
                        if (_attributedChangedObservers.ContainsKey(mel.Id))
                            list.Add(mel);
                    }
                }
                finally
                {
                    _attributedChangedObserversSync.ExitReadLock();
                }

                foreach (var mel in list)
                {
                    var notifier = mel as IPropertyChangedNotifier;
                    if (notifier != null)
                    {
                        notifier.NotifyPropertyChanged(cmd.PropertyName);
                    }
                }
            }
        }

        #endregion
    }
}