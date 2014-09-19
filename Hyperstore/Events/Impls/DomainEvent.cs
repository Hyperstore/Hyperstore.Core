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
using System.Diagnostics;

#endregion

namespace Hyperstore.Modeling.Events
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A domain event.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.IEvent"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class AbstractDomainEvent : IEvent
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected AbstractDomainEvent()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="extensionName">
        ///  Name of the extension.
        /// </param>
        /// <param name="correlationId">
        ///  The correlation identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected AbstractDomainEvent(string domainModel, string extensionName, Guid correlationId)
            : this(domainModel, extensionName, 0, correlationId)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="extensionName">
        ///  Name of the extension.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        /// <param name="correlationId">
        ///  The correlation identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected AbstractDomainEvent(string domainModel, string extensionName, long version, Guid correlationId)
        {
            Contract.RequiresNotEmpty(domainModel, "domainModel");

            DomainModel = domainModel;
            CorrelationId = correlationId;
            Version = version;
            ExtensionName = extensionName;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the version.
        /// </summary>
        /// <value>
        ///  The version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long Version
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [is top level event].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [is top level event]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsTopLevelEvent
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the correlation identifier.
        /// </summary>
        /// <value>
        ///  The correlation identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid CorrelationId
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string DomainModel
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the name of the extension.
        /// </summary>
        /// <value>
        ///  The name of the extension.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string ExtensionName
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }
    }
}