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
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A channel policy.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class ChannelPolicy
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="propagationStrategy">
        ///  (Optional) The propagation strategy.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ChannelPolicy(EventPropagationStrategy propagationStrategy = EventPropagationStrategy.TopLevelOnly)
        {
            PropagationStrategy = propagationStrategy;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the propagation strategy.
        /// </summary>
        /// <value>
        ///  The propagation strategy.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public EventPropagationStrategy PropagationStrategy { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the filter.
        /// </summary>
        /// <value>
        ///  The filter.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Func<IEvent, bool> Filter { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determine if we should be propagated.
        /// </summary>
        /// <param name="event">
        ///  The event.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool ShouldBePropagated(IEvent @event)
        {
            return (
                    PropagationStrategy == EventPropagationStrategy.All 
                        || (PropagationStrategy == EventPropagationStrategy.TopLevelOnly && @event.IsTopLevelEvent)
                   )
                   && (Filter == null || Filter(@event));
        }

        internal bool CanReceive(IEvent evt)
        {
            return true;
        }
    }
}