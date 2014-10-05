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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An in proc channel.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Messaging.AbstractChannel"/>
    ///-------------------------------------------------------------------------------------------------
    public class InProcChannel : AbstractChannel
    {
        private static readonly BlockingCollection<InprocMessage> EventQueue = new BlockingCollection<InprocMessage>(1);
        private static ISubjectWrapper<InprocMessage> _messages;
        private static readonly CancellationTokenSource TokenSource;

        static InProcChannel()
        {
            TokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                while (!TokenSource.IsCancellationRequested && !EventQueue.IsCompleted)
                {
                    InprocMessage msg;
                    if (EventQueue.TryTake(out msg, -1, TokenSource.Token) && _messages != null) // TODO verif charge CPU 
                        _messages.OnNext(msg);
                }
            }, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Starts the asynchronous.
        /// </summary>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override Task StartAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            _messages = new Subject<InprocMessage>(EventBus.Store.Services);
            try
            {
                if (EventsProcessor != null)
                {
                    if (HasInputProperty())
                    {
                        // Reception des messages
                        _messages.Subscribe(msg =>
                            EventsProcessor.ProcessEvents(msg.OriginStoreId, msg.Mode, msg.Events)
                            );
                    }
                }
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
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
        protected override Message SendMessage(Guid originStoreId, SessionMode mode, int sessionId, IEnumerable<IEvent> events)
        {
            EventQueue.Add(new InprocMessage { Mode = mode, OriginStoreId = originStoreId, Events = events.ToList() });
            return null;
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
            if (TokenSource != null)
                TokenSource.Cancel();

            if (_messages != null)
                _messages.Dispose();
            _messages = null;

            EventQueue.CompleteAdding();
        }

        private class InprocMessage
        {
            internal SessionMode Mode { get; set; }
            internal Guid OriginStoreId { get; set; }
            internal List<IEvent> Events { get; set; }
        }
    }
}
