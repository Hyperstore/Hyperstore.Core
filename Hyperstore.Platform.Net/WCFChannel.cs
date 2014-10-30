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
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A WCF channel.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Messaging.AbstractChannel"/>
    /// <seealso cref="T:Hyperstore.Modeling.Messaging.IWCFHyperstoreChannel"/>
    ///-------------------------------------------------------------------------------------------------
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WCFChannel : AbstractChannel, IWCFHyperstoreChannel
    {
        private ServiceEndpoint _endpoint;
        private DuplexChannelFactory<IWCFHyperstoreChannel> _factory;
        private ServiceHost _host;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="address">
        ///  The address.
        /// </param>
        /// <param name="binding">
        ///  The binding.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public WCFChannel(Uri address, Binding binding)
        {
            Contract.Requires(address, "address");
            Contract.Requires(binding, "binding");

            DefaultAddress = address;
            DefaultBinding = binding;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the channel.
        /// </summary>
        /// <value>
        ///  The channel.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IWCFHyperstoreChannel Channel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the default address.
        /// </summary>
        /// <value>
        ///  The default address.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Uri DefaultAddress { get; protected set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the default binding.
        /// </summary>
        /// <value>
        ///  The default binding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Binding DefaultBinding { get; protected set; }

        /// <summary>
        ///     Remote call
        ///     Reception des messages distants
        /// </summary>
        /// <param name="data"></param>
        void IWCFHyperstoreChannel.ProcessEvents(Message data)
        {
            DebugContract.Requires(data);

            if (IsDisposed || EventsProcessor == null)
                return;

            var events = new List<IEvent>();

            foreach (var envelope in data.events)
            {
                var @event = envelope.DeserializeEvent();
                if (@event == null)
                {
                    Trace.WriteLine("Ignore event : " + envelope.eventName);
                    continue;
                }

                if (CanReceive(@event))
                    events.Add(@event);
            }

            EventsProcessor.ProcessEvents(data.origin, data.sessionMode, events);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates end point.
        /// </summary>
        /// <returns>
        ///  The new end point.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ServiceEndpoint CreateEndPoint()
        {
            return new ServiceEndpoint(ContractDescription.GetContract(typeof(IWCFHyperstoreChannel)), DefaultBinding, new EndpointAddress(DefaultAddress));
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
        protected override Message SendMessage(string originStoreId, ISessionInformation session, IEnumerable<IEvent> events)
        {
            var msg = base.SendMessage(originStoreId, session, events);
            Channel.ProcessEvents(msg);
            return msg;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Configure channel.
        /// </summary>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override async Task ConfigureChannel()
        {
            _endpoint = CreateEndPoint();
            _host = new ServiceHost(this);
            _factory = CreateFactoryChannel();

            var channel = _factory.CreateChannel();
            Channel = channel;

            var comObj = channel as ICommunicationObject;

            await Task.Factory.FromAsync(comObj.BeginOpen, comObj.EndOpen, null).ConfigureAwait(false);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates factory channel.
        /// </summary>
        /// <returns>
        ///  The new factory channel.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual DuplexChannelFactory<IWCFHyperstoreChannel> CreateFactoryChannel()
        {
            return new DuplexChannelFactory<IWCFHyperstoreChannel>(new InstanceContext(this), _endpoint);
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
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if( Channel != null)
                ((ICommunicationObject)Channel).Close();
            if (_factory != null)
                _factory.Close();
        }
    }
}