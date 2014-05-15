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
using Hyperstore.Modeling.Events;
using System.Threading;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    internal class EventsProcessor : IEventsProcessor
    {
        struct ProcessInfo
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  The origin.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public Guid Origin;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  The mode.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public SessionMode Mode;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  The events.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public IEnumerable<IEvent> Events;
        }

        private readonly IConcurrentQueue<ProcessInfo> _processes;
        private readonly object _gate = new object();
        private readonly IHyperstore _store;
        private IEventDispatcher _defaultDispatcher;

        internal EventsProcessor(IHyperstore store)
        {
            DebugContract.Requires(store);
            _processes = PlatformServices.Current.CreateConcurrentQueue<ProcessInfo>();
            _store = store;
        }

        void IEventsProcessor.ProcessEvents(Guid origin, SessionMode mode, IEnumerable<IEvent> events)
        {
            ProcessInfo pi;
            pi.Events = events;
            pi.Origin = origin;
            pi.Mode = mode;
            _processes.Enqueue(pi);

            if (!Monitor.TryEnter(_gate, 10))
                return;

            try
            {
                while (!_processes.IsEmpty)
                {
                    ProcessInfo info;
                    if (_processes.TryDequeue(out info))
                    {
                        ProcessEvents(info);
                    }
                }
            }
            finally
            {
                Monitor.Exit(_gate);
            }
        }

        private void ProcessEvents(ProcessInfo info)
        {
            if (info.Origin == _store.Id)
                return;

            var tx = _store.BeginSession(new SessionConfiguration
                                                     {
                                                         IsolationLevel = SessionIsolationLevel.Serializable,
                                                         Mode = info.Mode
                                                     });

            ((ISessionInternal)tx).SetOriginStoreId(info.Origin);
            _store.Trace.WriteTrace(TraceCategory.EventBus, "Process events from {0} for Store {1}", info.Origin, _store.Id);

            try
            {
                foreach (var @event in info.Events)
                {
                    if (@event == null)
                        continue;

                    _store.Trace.WriteTrace(TraceCategory.EventBus, "Process event : " + @event);
                    var dispatcher = GetEventDispatcher(@event);
                    if (dispatcher != null)
                        dispatcher.HandleEvent(@event);
                }

                tx.AcceptChanges();
            }
            catch (Exception ex)
            {
                ((ISessionInternal)tx).SessionContext.Log(new DiagnosticMessage(MessageType.Error, "ProcessEvents", "EventProcessor", null, ex));
                throw;
            }
            finally
            {
                tx.Dispose();
            }
        }

        private IEventDispatcher GetEventDispatcher(IEvent @event)
        {
            var domainModel = _store.GetDomainModel(@event.DomainModel);
            if (domainModel.EventDispatcher != null)
                return domainModel.EventDispatcher;
            if (_defaultDispatcher == null)
            {
                _defaultDispatcher = _store.DependencyResolver.Resolve<IEventDispatcher>();
                DebugContract.Assert(_defaultDispatcher != null);
            }
            return _defaultDispatcher;
        }
    }
}