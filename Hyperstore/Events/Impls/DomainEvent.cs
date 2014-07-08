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