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
            return InputProperty != null && (DomainModel == null || String.Compare(DomainModel.Name, evt.DomainModel, StringComparison.OrdinalIgnoreCase) == 0) && InputProperty.CanReceive(evt);
        }
    }
}
