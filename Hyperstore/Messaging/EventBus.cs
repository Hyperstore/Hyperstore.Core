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

using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    /// <summary>
    /// </summary>
    internal sealed class EventBus : IDisposable, IEventBus
    {
        private readonly object _sync = new object();
        private List<IEventBusChannel> _channels;
        private readonly Dictionary<IDomainModel, PoliciesInfo> _policies = new Dictionary<IDomainModel, PoliciesInfo>();

        private class PoliciesInfo
        {
            public ChannelPolicy Output;
            public ChannelPolicy Input;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in EventBusStarted events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler EventBusStarted;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the channels.
        /// </summary>
        /// <value>
        ///  The channels.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IEventBusChannel> Channels { get { return _channels; } }

        private bool _disposed;
        private readonly IHyperstore _store;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store { get { return _store; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default event dispatcher.
        /// </summary>
        /// <value>
        ///  The default event dispatcher.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEventDispatcher DefaultEventDispatcher
        {
            get;
            private set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="services">
        ///  The services.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EventBus(IServicesContainer services)
        {
            DebugContract.Requires(services);
            _store = services.Resolve<IHyperstore>();
            DefaultEventDispatcher = services.Resolve<IEventDispatcher>();
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
        ///  Starts the asynchronous.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///  Thrown when a supplied object has been disposed.
        /// </exception>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public async Task OpenAsync(params IEventBusChannel[] channels)
        {
            if (_disposed)
                throw new ObjectDisposedException("EventBus");

            List<IEventBusChannel> channelList = null;
            lock (_sync)
            {
                if (_channels == null)
                    _channels = new List<IEventBusChannel>();

                _channels.AddRange(channels);
                channelList = _channels.ToList();
            }

            if (channelList == null || channelList.Count == 0)
            {
                OnEventBusStarted();
                return;
            }

            Dictionary<IDomainModel, PoliciesInfo> policies;
            lock (_policies)
            {
                policies = new Dictionary<IDomainModel, PoliciesInfo>(_policies);
            }

            if (policies.Count == 0)
            {
                OnEventBusStarted();
                return;
            }

            Task[] tasks = new Task[channelList.Count];
            var i = 0;
            foreach (var channel in channelList)
            {
                foreach(var info in policies)
                {
                    channel.RegisterFilter(new ChannelFilter(info.Value.Output, info.Value.Input, info.Key));
                }

                tasks[i++] = channel.StartAsync(this);
            }

            await Task.WhenAll(tasks);

            _store.SessionCompleting += OnSessionCompleted;

            OnEventBusStarted();
        }

        private void OnEventBusStarted()
        {
            var tmp = EventBusStarted;
            if( tmp !=null)
            {
                tmp(this, new EventArgs());
            }
        }

        private void OnSessionCompleted(object sender, SessionCompletingEventArgs e)
        {
            if (_channels == null || e.Session.IsAborted)
                return;

            PlatformServices.Current.Parallel_ForEach(_channels, channel => channel.SendEvents(e.Session));
        }

        private void Dispose(bool releaseManagedResources)
        {
            if (_disposed)
                return;

            _disposed = true;

            _store.SessionCompleting -= OnSessionCompleted;


            if (_channels != null)
            {
                foreach (var item in _channels)
                {
                    try
                    {
                        var disposable = item as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            _channels = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers this instance.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="outputProperty">
        ///  The output property.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterDomainPolicies(IDomainModel domain, ChannelPolicy outputProperty, ChannelPolicy inputProperty = null)
        {
            Contract.Requires(domain, "domain");
            if (outputProperty == null && inputProperty == null)
                return;

            lock (_policies)
            {
                PoliciesInfo info;
                if (_policies.TryGetValue(domain, out info))
                {
                    info.Input = inputProperty;
                    info.Output = outputProperty;
                    return;
                }

                info = new PoliciesInfo { Output = outputProperty, Input = inputProperty };
                _policies.Add(domain, info);
            }
        }
    }
}