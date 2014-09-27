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