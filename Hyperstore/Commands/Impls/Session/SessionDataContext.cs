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
using Hyperstore.Modeling.Metadata.Constraints;

#endregion

namespace Hyperstore.Modeling.Commands
{
    /// <summary>
    ///     Structure des données spécifiques à la transaction
    /// </summary>
    internal class SessionDataContext
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public readonly IList<IEvent> Events = new List<IEvent>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  TODO  Données utilisateurs  (session specific ?)
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public readonly Dictionary<string, object> Infos = new Dictionary<string, object>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  List of messages.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public readonly ExecutionResult MessageList = new ExecutionResult();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Rollback demandé.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public bool Aborted;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The cancellation token.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public CancellationToken CancellationToken;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The command execution scope level.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public int CommandExecutionScopeLevel;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Singleton (par thread)
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public Session Current;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true to disposing.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public bool Disposing;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The enlistment.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public List<ISessionEnlistmentNotification> Enlistment;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true to in validation process.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public bool InValidationProcess;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The locks.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public List<ILockInfo> Locks;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The mode.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public SessionMode Mode;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Identifier for the origin store.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public Guid OriginStoreId;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true to read only.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public bool ReadOnly;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Valeur initiale au démarrage de la session.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public bool ReadOnlyStatus;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Identifier for the session.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public Guid SessionId;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The top level session.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public ISession TopLevelSession;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The session infos.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public Stack<SessionLocalInfo> SessionInfos;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Indique qu'on est dans le process de validation (Sert à 'marquer' les messages)
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public SessionIsolationLevel SessionIsolationLevel;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The store.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Information describing the tracking.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public SessionTrackingData TrackingData;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the depth.
        /// </summary>
        /// <value>
        ///  The depth.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Depth
        {
            get { return SessionInfos.Count; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The trackers.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public Stack<CalculatedProperty> Trackers;
    }
}