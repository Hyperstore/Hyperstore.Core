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

#endregion

namespace Hyperstore.Modeling.Scopes
{
    /// <summary>
    ///     Information sur un domaine et ses extensions
    /// </summary>
    internal class ExtensionInfo<T> : ScopeInfo<T>, IDomainInfos<T> where T : class, IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="extension">
        ///  The extension.
        /// </param>
        /// <param name="activeSessions">
        ///  The active sessions.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ExtensionInfo(T extension, List<int> activeSessions)
            : base(extension)
        {
            PendingLoadSessions = activeSessions;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Active le nouveau contexte (le domaine ou son extension)
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Activate(T domain)
        {
            if (domain.InstanceId == DomainModel.InstanceId)
            {
                DebugContract.Requires(Status != ScopeStatus.ScopeEnabled);
                DebugContract.Requires(DomainModel != null);
                Status = ScopeStatus.ScopeEnabled;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Fournit le domaine ou son extension en tenant compte du status courant.
        /// </summary>
        /// <param name="sessionId">
        ///  .
        /// </param>
        /// <returns>
        ///  The domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetDomainModel(int sessionId)
        {
            //DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, " >>> GetDomainModel extension for session {0} - ALT is {1}", sessionId, DomainModel == null ? "null" : "not null");
            //if (PendingUnloadSessions != null)
            //    DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "      -- unload : {0}", String.Join(",", PendingUnloadSessions));
            //if (PendingLoadSessions != null)
            //    DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "      -- load : {0}", String.Join(",", PendingLoadSessions));

            if (DomainModel != null
                && ( Status == ScopeStatus.ScopeEnabled) && (PendingUnloadSessions == null || sessionId == 0 || PendingUnloadSessions.Contains(sessionId))
                && (PendingLoadSessions == null || sessionId == 0 || !PendingLoadSessions.Contains(sessionId) || (Session.Current.Mode & SessionMode.LoadingSchema) == SessionMode.LoadingSchema)) // TODO ou Loading ???
            {
                //DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "  return ALT");
                return DomainModel;
            }

            return null;
        }

        public bool IsExtensionNameExists(string name)
        {
            var dm = DomainModel;
            return String.Compare( dm.ExtensionName, name, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}