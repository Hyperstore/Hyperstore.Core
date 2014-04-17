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
using System.Collections;
using System.Threading;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Paramètres de configuration d'une session.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class SessionConfiguration
    {
        private const int DomainModelIndex = 0;
        private const int IsolationLevelIndex = 1;
        private const int TimeoutIndex = 2;
        private readonly BitArray _settings = new BitArray(8);

        private IDomainModel _domainModel;
        private SessionIsolationLevel _isolationLevel;
        private TimeSpan _timeOut;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="SessionConfiguration" /> class.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public SessionConfiguration()
        {
            CancellationToken = CancellationToken.None;
            IsolationLevel = SessionIsolationLevel.ReadCommitted;
#if !TEST
            SessionTimeout = TimeSpan.FromMinutes(1);
#else
            Timeout = TimeSpan.Zero;
#endif
            Readonly = false;
            Mode = SessionMode.Normal;
            SessionId = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the default domain model for all elements created during this session.
        /// </summary>
        /// <value>
        ///  The default domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DefaultDomainModel
        {
            get { return _domainModel; }
            set
            {
                _domainModel = value;
                _settings.Set(DomainModelIndex, value != null);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        ///  The cancellation token.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public CancellationToken CancellationToken { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the isolation level.
        /// </summary>
        /// <value>
        ///  The isolation level.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionIsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set
            {
                _isolationLevel = value;
                _settings.Set(IsolationLevelIndex, true);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the timeout.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///  Thrown when one or more arguments are outside the required range.
        /// </exception>
        /// <value>
        ///  The timeout.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public TimeSpan SessionTimeout
        {
            get { return _timeOut; }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("Timeout");
                _timeOut = value;
                _settings.Set(TimeoutIndex, true);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets a value indicating whether this <see cref="SessionConfiguration" /> is readonly.
        /// </summary>
        /// <value>
        ///  <c>true</c> if readonly; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool Readonly { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the mode.
        /// </summary>
        /// <value>
        ///  The mode.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionMode Mode { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the session id.
        /// </summary>
        /// <value>
        ///  The session id.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid? SessionId { get; set; }

        /// <summary>
        ///     Merges the specified configuration.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        /// <returns></returns>
        internal SessionConfiguration Merge(SessionConfiguration cfg)
        {
            var sc = new SessionConfiguration();
            sc.Readonly = cfg.Readonly;
            sc.CancellationToken = cfg.CancellationToken;
            sc.SessionId = cfg.SessionId;
            sc.Mode = cfg.Mode | Mode;

            // Overridable properties
            sc.DefaultDomainModel = cfg._settings.Get(DomainModelIndex) ? cfg.DefaultDomainModel : DefaultDomainModel;
            sc.SessionTimeout = cfg._settings.Get(TimeoutIndex) ? cfg.SessionTimeout : SessionTimeout;
            sc.IsolationLevel = cfg._settings.Get(IsolationLevelIndex) ? cfg.IsolationLevel : IsolationLevel;

            return sc;
        }
    }
}