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
                _defaultDispatcher = _store.Services.Resolve<IEventDispatcher>();
                DebugContract.Assert(_defaultDispatcher != null);
            }
            return _defaultDispatcher;
        }
    }
}