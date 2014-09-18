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
using System.Collections.Generic;
using System.Threading;
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Session information.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface ISessionInformation
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the tracking data - All elements involved by the session.
        /// </summary>
        /// <value>
        ///  Information describing the tracking.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ISessionTrackingData TrackingData { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the cancellation token.
        /// </summary>
        /// <value>
        ///  The cancellation token.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        CancellationToken CancellationToken { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether the session is aborted.
        /// </summary>
        /// <value>
        ///  true if this instance is aborted, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsAborted { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this session has errors.
        /// </summary>
        /// <value>
        ///  true if this instance has errors, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool HasErrors { get; }  

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this session has warnings.
        /// </summary>
        /// <value>
        ///  true if this instance has warnings, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool HasWarnings { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Session has completed correctly (no errors, no warnings and not aborted)
        /// </summary>
        /// <value>
        ///  true if succeed, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool Succeed { get; }  

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IHyperstore Store { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event list.
        /// </summary>
        /// <value>
        ///  The events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IEvent> Events { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  ID of the store which has generated all the commands.
        /// </summary>
        /// <value>
        ///  The identifier of the origin store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Guid OriginStoreId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session id.
        /// </summary>
        /// <value>
        ///  The identifier of the session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Guid SessionId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session mode.
        /// </summary>
        /// <value>
        ///  The mode.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        SessionMode Mode { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this session is nested.
        /// </summary>
        /// <value>
        ///  true if this instance is nested, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsNested { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default domain model.
        /// </summary>
        /// <value>
        ///  The default domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IDomainModel DefaultDomainModel { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether the session is read only.
        /// </summary>
        /// <value>
        ///  true if this instance is read only, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsReadOnly { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the context info.
        /// </summary>
        /// <typeparam name="T">
        ///  Type of the element to retrieve.
        /// </typeparam>
        /// <param name="key">
        ///  Key of the element.
        /// </param>
        /// <returns>
        ///  The context information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T GetContextInfo<T>(string key);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a value in the context info.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void SetContextInfo(string key, object value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Logs the given message.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Log(DiagnosticMessage message);
    }
}