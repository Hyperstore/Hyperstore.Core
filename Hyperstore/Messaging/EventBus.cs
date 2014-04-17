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

using Hyperstore.Modeling.Events;
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
        private Dictionary<IDomainModel, PoliciesInfo> _policies = new Dictionary<IDomainModel, PoliciesInfo>();

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
        private IHyperstore _store;

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
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EventBus(IDependencyResolver resolver)
        {
            DebugContract.Requires(resolver);
            _store = resolver.Resolve<IHyperstore>();
            DefaultEventDispatcher = resolver.Resolve<IEventDispatcher>();
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

                tasks[i] = channel.StartAsync(this);
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

            Parallel.ForEach(_channels, channel => channel.SendEvents(e.Session));
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