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
    internal abstract class ScopeInfo<T> where T : class, IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The domain model.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected T DomainModel;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sessions actives lors du chargement de l'extension du domaine.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected List<int> PendingLoadSessions;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sessions actives lors du déchargement d'un domaine.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected List<int> PendingUnloadSessions;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected ScopeInfo(T domain)
        {
            DebugContract.Requires(domain!=null);
            DomainModel = domain;
            Status = ScopeStatus.Disabled;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the status.
        /// </summary>
        /// <value>
        ///  The status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected ScopeStatus Status { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Méthode appelée à la fin de chaque session.
        /// </summary>
        /// <param name="sessionId">
        ///  .
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool OnSessionCompleted(int sessionId)
        {
            DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "Session completed for {0}", sessionId);

            if (PendingLoadSessions != null)
            {
                DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, " Update load sessions {0}", String.Join(",", PendingLoadSessions));
                PendingLoadSessions.Remove(sessionId);
                if (PendingLoadSessions.Count == 0)
                    PendingLoadSessions = null;
            }

            if (PendingUnloadSessions != null)
            {
                PendingUnloadSessions.Remove(sessionId);
                DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, " Update unload sessions {0}", String.Join(",", PendingUnloadSessions));
                if (PendingUnloadSessions.Count == 0)
                {
                    if (PendingLoadSessions == null)
                    {
                        DebugContract.Requires(Status == ScopeStatus.ScopeEnabled);
                        DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, " - Set extensionDomain to null");
                        PendingUnloadSessions = null; // Raz que si _pendingLoadSessions est null

                        OnUnload();
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnUnload()
        {
            Status = GetStatusAfterUnload();
            DomainModel.Dispose();
            DomainModel = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets status after unload.
        /// </summary>
        /// <returns>
        ///  The status after unload.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ScopeStatus GetStatusAfterUnload()
        {
            return ScopeStatus.Enabled;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unloads.
        /// </summary>
        /// <param name="activeSessions">
        ///  The active sessions.
        /// </param>
        /// <param name="extension">
        ///  The extension.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual bool Unload(List<int> activeSessions, T extension)
        {
            if (extension == null || extension.InstanceId == DomainModel.InstanceId)
            {
                DebugContract.Requires(activeSessions);
                DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "*** Unload extension for {1} with active sessions {0}", String.Join(",", activeSessions), DomainModel.Name);

                if (PendingUnloadSessions != null)
                {
                    DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "- Abort extension with pending unload sessions {0}", String.Join(",", PendingUnloadSessions));
                    // On est dèja en train de décharger ????
                    return false;
                }

                DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, " - OK");
                // Sessions actives au moment du déchargement de l'extension
                if (activeSessions.Count > 0)
                {
                    PendingUnloadSessions = new List<int>(activeSessions);
                    return false;
                }

                OnUnload();
                return true;
            }
            return false;
        }
    }
}