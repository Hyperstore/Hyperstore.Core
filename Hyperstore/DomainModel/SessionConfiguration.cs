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

            Readonly = false;
            Mode = SessionMode.Normal;
            SessionId = 0;
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
        public int SessionId { get; set; }

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