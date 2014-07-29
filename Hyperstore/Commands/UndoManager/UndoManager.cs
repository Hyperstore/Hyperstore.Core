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
using System.ComponentModel;
using System.Linq;
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Manager for undoes.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.IUndoManager"/>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public class UndoManager : IUndoManager, INotifyPropertyChanged
    {
        private readonly Dictionary<string, DomainInfo> _domainModels;
        private readonly RecursiveStack<SessionEvents> _redos;
        private readonly IHyperstore _store;
        private readonly RecursiveStack<SessionEvents> _undos;
        private readonly object _sync = new object();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Capacity of the stack.
        /// </summary>
        /// <value>
        ///  The capacity.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Capacity { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="UndoManager" /> class.
        /// </summary>
        /// <param name="store">
        ///  The store.
        /// </param>
        /// <param name="capacity">
        ///  (Optional)
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public UndoManager(IHyperstore store, int capacity = 100)
        {
            Contract.Requires(store, "store");
            Contract.Requires(capacity > 0, "capacity");

            _store = store;
            _domainModels = new Dictionary<string, DomainInfo>();
            _undos = new RecursiveStack<SessionEvents>(capacity);
            _redos = new RecursiveStack<SessionEvents>(capacity);
            Capacity = capacity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Occurs when a property value changes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the events dispatcher associated with a domain model.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <returns>
        ///  The event dispatcher.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEventDispatcher GetEventDispatcher(IDomainModel domainModel)
        {
            DomainInfo info;
            if (_domainModels.TryGetValue(domainModel.Name, out info))
            {
                return info.Dispatcher;
            }
            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets a value indicating whether [enabled].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [enabled]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool Enabled { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the save point.
        /// </summary>
        /// <value>
        ///  The save point.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid? SavePoint
        {
            get
            {
                lock (_sync)
                {
                    return _undos.Count > 0
                            ? (Guid?)_undos.Peek()
                                    .SessionId
                            : Guid.Empty;
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the domain.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="eventDispatcher">
        ///  (Optional) the event dispatcher.
        /// </param>
        /// <param name="eventFilter">
        ///  (Optional) a filter specifying the event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterDomain(IDomainModel domainModel, IEventDispatcher eventDispatcher = null, Func<IUndoableEvent, bool> eventFilter = null)
        {
            Contract.Requires(domainModel, "domainModel");

            if (_domainModels.ContainsKey(domainModel.Name))
                return;

            DomainInfo info;
            info.Dispatcher = (eventDispatcher ?? domainModel.EventDispatcher) ?? _store.EventBus.DefaultEventDispatcher;
            info.Filter = eventFilter;
            _domainModels.Add(domainModel.Name, info);
            Enabled = true;
            domainModel.Events.SessionCompleted.Subscribe(s =>
            {
                if (!s.IsAborted && Enabled && (s.Mode & SessionMode.UndoOrRedo) == SessionMode.Normal)
                    Push(s);
            });
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Undoes this instance.
        /// </summary>
        /// <param name="toSavePoint">
        ///  (Optional) to save point.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Undo(Guid? toSavePoint = null)
        {
            if (CanUndo)
                PerformPop(_undos, _redos, SessionMode.Undo, toSavePoint);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [can undo].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [can undo]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual bool CanUndo
        {
            get { return _undos.Count > 0; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Redoes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Redo()
        {
            if (CanRedo)
                PerformPop(_redos, _undos, SessionMode.Redo, null);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [can redo].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [can redo]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual bool CanRedo
        {
            get { return _redos.Count > 0; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Empile les événements de la session en agrégeant tous les événements sur le même n° de
        ///  session.
        /// </summary>
        /// <param name="session">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Push(ISessionInformation session)
        {
            if (session.IsReadOnly || (session.Mode & SessionMode.Loading) == SessionMode.Loading)
                return;

            var eventsQuery = session.Events.OfType<IUndoableEvent>()
                .Where(e =>
                    {
                        /*e.IsTopLevelEvent &&*/
                        DomainInfo info;
                        if (!_domainModels.TryGetValue(e.DomainModel, out info))
                            return false;
                        return info.Filter == null || info.Filter(e);
                    });

            var events = eventsQuery.ToList();

            if (events.Count > 0)
            {
                lock (_sync)
                {
                    _redos.Clear();
                    OnPropertyChanged("CanRedo");

                    // Est ce qu'il existe une session identique ?
                    foreach (var ci in _undos)
                    {
                        if (ci.SessionId == session.SessionId)
                        {
                            ci.Events.AddRange(events);
                            return;
                        }
                    }

                    _undos.Push(new SessionEvents
                                {
                                    SessionId = session.SessionId,
                                    Events = events.ToList()
                                });
                }
            }
            OnPropertyChanged("CanUndo");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates filter events in this collection.
        /// </summary>
        /// <param name="events">
        ///  The events.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process filter events in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IEnumerable<IUndoableEvent> FilterEvents(IEnumerable<IUndoableEvent> events)
        {
            return events;
        }

        private void PerformPop(RecursiveStack<SessionEvents> mainStack, RecursiveStack<SessionEvents> altStack, SessionMode mode, Guid? toSavePoint)
        {
            var notify = false;
            lock (_sync)
            {
                Guid? sid = null;
                var events = new List<IUndoableEvent>();
                using (var session = _store.BeginSession(new SessionConfiguration { Mode = mode }))
                {
                    IEventDispatcher dispatcher = null;
                    string domainModelName = null;
                    while (mainStack.Count > 0)
                    {
                        if (toSavePoint != null && mainStack.Peek().SessionId == toSavePoint.Value)
                            break;

                        var ci = mainStack.Pop();
                        foreach (var @event in Enumerable.Reverse(ci.Events))
                        {
                            var evt = @event.GetReverseEvent(session.SessionId);
                            if (evt == null)
                                continue;
                            if (domainModelName != evt.DomainModel)
                            {
                                dispatcher = _domainModels[evt.DomainModel].Dispatcher;
                                domainModelName = evt.DomainModel;
                            }
                            dispatcher.HandleEvent(evt);

                            if (evt is IUndoableEvent)
                                events.Add(evt as IUndoableEvent);
                        }

                        sid = ci.SessionId;
                        if (toSavePoint == null)
                            break;
                    }

                    session.AcceptChanges();
                }

                if (events.Count > 0)
                {
                    notify = true;
                    altStack.Push(new SessionEvents
                                  {
                                      SessionId = sid.Value,
                                      Events = events.ToList()
                                  });
                }
            }

            if (notify)
            {
                OnPropertyChanged("CanUndo");
                OnPropertyChanged("CanRedo");
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Clears this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Clear()
        {
            lock (_sync)
            {
                _undos.Clear();
                _redos.Clear();
            }
            OnPropertyChanged("CanUndo");
            OnPropertyChanged("CanRedo");
        }

        private void OnPropertyChanged(string propertyName)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
                tmp(this, new PropertyChangedEventArgs(propertyName));
        }

        private struct SessionEvents
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Identifier for the session.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public Guid SessionId;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets or sets the events.
            /// </summary>
            /// <value>
            ///  The events.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public List<IUndoableEvent> Events { get; set; }
        }

        private struct DomainInfo
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  The dispatcher.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public IEventDispatcher Dispatcher;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Specifies the filter.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public Func<IUndoableEvent, bool> Filter;
        }
    }
}