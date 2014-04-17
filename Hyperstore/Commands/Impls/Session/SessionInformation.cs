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
using System.Linq;
using System.Threading;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling
{
    /// <summary>
    ///     Wrapper en read only du résultat d'une session.
    ///     Cette classe est nécessaire car elle est utilisée par les événements.
    ///     Comme il est possible que des événements soient traités en asynchrone, la session initiale n'existera plus à ce
    ///     moment là il en faut
    ///     donc une copie.
    /// </summary>
    internal class SessionInformation : ISessionInformation
    {
        #region Fields

        private readonly Dictionary<string, object> _contextInfos;
        private ISessionContext _context;
        #endregion

        #region Properties

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the tracking data - All elements involved by the session.
        /// </summary>
        /// <value>
        ///  Information describing the tracking.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISessionTrackingData TrackingData { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  ID of the store which has generated all the commands.
        /// </summary>
        /// <value>
        ///  The identifier of the origin store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid OriginStoreId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether the session is read only.
        /// </summary>
        /// <value>
        ///  true if this instance is read only, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsReadOnly { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the cancellation token.
        /// </summary>
        /// <value>
        ///  The cancellation token.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public CancellationToken CancellationToken { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session mode.
        /// </summary>
        /// <value>
        ///  The mode.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionMode Mode { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether the session is aborted.
        /// </summary>
        /// <value>
        ///  true if this instance is aborted, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsAborted { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event list.
        /// </summary>
        /// <value>
        ///  The events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IEvent> Events { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this session is nested.
        /// </summary>
        /// <value>
        ///  true if this instance is nested, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsNested { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session id.
        /// </summary>
        /// <value>
        ///  The identifier of the session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid SessionId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default domain model.
        /// </summary>
        /// <value>
        ///  The default domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DefaultDomainModel { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionInformation" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="info"></param>
        /// <param name="trackingData"></param>
        internal SessionInformation(Session session, SessionLocalInfo info, ISessionTrackingData trackingData)
        {
            DebugContract.Requires(session, "session");
            DebugContract.Requires(trackingData);

            _context = session.SessionContext;
            TrackingData = trackingData;
            CancellationToken = session.CancellationToken;
            IsAborted = session.IsAborted;
            IsNested = session.IsNested;
            Store = session.Store;
            IsReadOnly = session.IsReadOnly;
            Mode = info.Mode;
            OriginStoreId = session.OriginStoreId;
            SessionId = session.SessionId;
            DefaultDomainModel = info.DefaultDomainModel;
            _contextInfos = new Dictionary<string, object>(session.GetInfos());
            Events = session.Events.ToList();
        }

        #endregion

        #region Methods

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Logs the given message.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Log(DiagnosticMessage message)
        {
            Contract.Requires(message != null, "message");
            _context.Log(message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the context info.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <returns>
        ///  The context information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetContextInfo<T>(string key)
        {
            Contract.RequiresNotEmpty(key, "key");

            object result;
            if (_contextInfos.TryGetValue(key, out result))
                return (T) result;

            return default(T);
        }

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
        public void SetContextInfo(string key, object value)
        {
            Contract.RequiresNotEmpty(key, "key");

            if (value == null)
            {
                if (_contextInfos.ContainsKey(key))
                    _contextInfos.Remove(key);
            }
            else
                _contextInfos[key] = value;
        }

        #endregion
    }
}