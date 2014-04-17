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
 
#if !NETFX_CORE
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
            _messages = new Subject<InprocMessage>(EventBus.Store.DependencyResolver);
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
          protected override Message SendMessage(Guid originStoreId, SessionMode mode, Guid sessionId, IEnumerable<IEvent> events)
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

#endif