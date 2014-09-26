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
 
using Hyperstore.Modeling.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A channel filter.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class ChannelFilter
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the output property.
        /// </summary>
        /// <value>
        ///  The output property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ChannelPolicy OutputProperty { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the input property.
        /// </summary>
        /// <value>
        ///  The input property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ChannelPolicy InputProperty { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="outputProperty">
        ///  The output property.
        /// </param>
        /// <param name="inputProperty">
        ///  The input property.
        /// </param>
        /// <param name="domainModel">
        ///  (Optional) the domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ChannelFilter(ChannelPolicy outputProperty, ChannelPolicy inputProperty, IDomainModel domainModel=null)
        {
            OutputProperty = outputProperty;
            InputProperty = inputProperty;
            DomainModel = domainModel;
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
        public virtual bool ShouldBePropagated(IEvent evt)
        {
            return OutputProperty != null 
                && (DomainModel == null || String.Compare(DomainModel.Name, evt.DomainModel, StringComparison.OrdinalIgnoreCase) == 0)
                && (String.Compare(DomainModel.ExtensionName, evt.ExtensionName, StringComparison.OrdinalIgnoreCase) == 0)
                && OutputProperty.ShouldBePropagated(evt);
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
        public virtual bool CanReceive(IEvent evt)
        {
            return InputProperty != null 
                && (DomainModel == null || String.Compare(DomainModel.Name, evt.DomainModel, StringComparison.OrdinalIgnoreCase) == 0)
                && (String.Compare(DomainModel.ExtensionName, evt.ExtensionName, StringComparison.OrdinalIgnoreCase) == 0)
                && InputProperty.CanReceive(evt);
        }
    }
}
