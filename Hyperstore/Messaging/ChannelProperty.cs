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