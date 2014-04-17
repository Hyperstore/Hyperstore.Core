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
using System.Diagnostics;
using System.Linq;
using System.Threading;

#endregion

namespace Hyperstore.Modeling
{
    [DebuggerDisplay("SessionIndex = {SessionIndex}")]
    internal class HyperstoreSynchronizationContext : SynchronizationContext
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public HyperstoreSynchronizationContext(ushort index, SynchronizationContext ctx)
        {
            OldContext = ctx;
            SessionIndex = index;
            //            SessionIndex = Interlocked.Increment(ref _sequence);
            SetSynchronizationContext(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a context for the old.
        /// </summary>
        /// <value>
        ///  The old context.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SynchronizationContext OldContext { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the zero-based index of the session.
        /// </summary>
        /// <value>
        ///  The session index.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ushort SessionIndex { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, creates a copy of the synchronization context.
        /// </summary>
        /// <returns>
        ///  A new <see cref="T:System.Threading.SynchronizationContext" /> object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override SynchronizationContext CreateCopy()
        {
            if (OldContext != null)
                return OldContext.CreateCopy();
            return base.CreateCopy();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, responds to the notification that an operation has
        ///  completed.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public override void OperationCompleted()
        {
            if (OldContext != null)
                OldContext.OperationCompleted();
            else
                base.OperationCompleted();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, responds to the notification that an operation has
        ///  started.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public override void OperationStarted()
        {
            if (OldContext != null)
                OldContext.OperationStarted();
            else
                base.OperationStarted();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, dispatches an asynchronous message to a synchronization
        ///  context.
        /// </summary>
        /// <param name="d">
        ///  The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.
        /// </param>
        /// <param name="state">
        ///  The object passed to the delegate.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public override void Post(SendOrPostCallback d, object state)
        {
            Action continuation = () =>
            {
                SetSynchronizationContext(this);
                ((Action) state)();
            };
            if (OldContext != null)
                OldContext.Post(d, continuation);
            else
                base.Post(d, continuation);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, dispatches a synchronous message to a synchronization
        ///  context.
        /// </summary>
        /// <param name="d">
        ///  The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.
        /// </param>
        /// <param name="state">
        ///  The object passed to the delegate.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public override void Send(SendOrPostCallback d, object state)
        {
            Action continuation = () =>
            {
                SetSynchronizationContext(this);
                ((Action) state)();
            };
            if (OldContext != null)
                OldContext.Send(d, continuation);
            else
                base.Send(d, continuation);
        }
    }
}