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
using System.Threading.Tasks;
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Utils;
using System.Linq;
#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An abstract channel.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Messaging.IEventBusChannel"/>
    /// <seealso cref="T:System.IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class AbstractChannel : IEventBusChannel, IDisposable
    {
        private bool _disposed;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event bus.
        /// </summary>
        /// <value>
        ///  The event bus.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEventBus EventBus { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the filters.
        /// </summary>
        /// <value>
        ///  The filters.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected List<ChannelFilter> Filters { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the events processor.
        /// </summary>
        /// <value>
        ///  The events processor.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IEventsProcessor EventsProcessor { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IHyperstore Store
        {
            get { return EventBus.Store; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///  true if this instance is disposed, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected bool IsDisposed
        {
            get { return _disposed; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the filter described by filter.
        /// </summary>
        /// <param name="filter">
        ///  Specifies the filter.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterFilter(ChannelFilter filter)
        {
            Contract.Requires(filter, "filter");
            if (Filters == null)
                Filters = new List<ChannelFilter>();
            Filters.Add(filter);
        }

        void IEventBusChannel.SendEvents(ISessionInformation session)
        {
            DebugContract.Requires(session);
            SendEvents(session);
        }

        private void SendEvents(ISessionInformation session)
        {
            DebugContract.Requires(session);

            if (_disposed || Filters == null)
                return;

            var origin = session.OriginStoreId;
            if (origin != Store.Id)
                return;

            bool first = true;
            List<IEvent> events = new List<IEvent>();
            foreach (var evt in session.Events)
            {
                if (ShouldBePropagated(evt))
                {
                    if (first)
                        Store.Trace.WriteTrace(TraceCategory.EventBus, "Send events on channel {0} Store {1}", this.GetType().Name, Store.Id);
                    first = false;
                    Store.Trace.WriteTrace(TraceCategory.EventBus, "Prepare event {0}", evt);
                    events.Add(evt);
                }
            }

            if (events.Count > 0)
            {             
                SendMessage(origin, session.Mode, session.SessionId, events);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sends a message.
        /// </summary>
        /// <param name="originStoreId">
        ///  Identifier for the origin store.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        /// <param name="sessionId">
        ///  Identifier for the session.
        /// </param>
        /// <param name="events">
        ///  The events.
        /// </param>
        /// <returns>
        ///  A Message.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual Message SendMessage(Guid originStoreId, SessionMode mode, int sessionId, IEnumerable<IEvent> events)
        {
            var msg = new Message { Mode = mode, Events = new List<Enveloppe>() };
            msg.OriginStoreId = originStoreId;
            msg.CorrelationId = sessionId;
            msg.Events = events.Select(e => new Enveloppe(e)).ToList();
            return msg;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determine if we can receive.
        /// </summary>
        /// <param name="evt">
        ///  The event.
        /// </param>
        /// <returns>
        ///  true if we can receive, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected bool CanReceive(IEvent evt)
        {
            return Filters.Any(f=>f.CanReceive(evt));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determine if we should be propagated.
        /// </summary>
        /// <param name="evt">
        ///  The event.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected bool ShouldBePropagated(IEvent evt)
        {
            return Filters.Any(f=>f.ShouldBePropagated(evt));
        }

        async Task IEventBusChannel.StartAsync(IEventBus eventBus)
        {
            DebugContract.Requires(eventBus);
            EventBus = eventBus;
            EventsProcessor = EventBus.Store.Services.Resolve<IEventsProcessor>() ?? new EventsProcessor(EventBus.Store);

            await StartAsync();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Starts the asynchronous.
        /// </summary>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual async Task StartAsync()
        {
            _disposed = false;

            // If at least one filter
            if (HasInputProperty() || HasOutputProperty() )
                await ConfigureChannel();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if this instance has output property.
        /// </summary>
        /// <returns>
        ///  true if output property, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected bool HasOutputProperty()
        {
            return Filters != null && Filters.Any(f => f.OutputProperty != null);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if this instance has input property.
        /// </summary>
        /// <returns>
        ///  true if input property, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected bool HasInputProperty()
        {
            return Filters != null && Filters.Any(f => f.InputProperty != null);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Configure channel.
        /// </summary>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual Task ConfigureChannel()
        {
            return CompletedTask.Default;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.Messaging.AbstractChannel
        ///  and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            Filters.Clear();
            _disposed = true;
            EventBus = null;
        }
    }
}