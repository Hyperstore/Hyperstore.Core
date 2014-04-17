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

#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal abstract class InfosBase<T> where T : class, IDomainModel
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
        protected List<Guid> PendingLoadSessions;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sessions actives lors du déchargement d'un domaine.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected List<Guid> PendingUnloadSessions;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected InfosBase(T domain)
        {
            DebugContract.Requires(domain!=null);
            DomainModel = domain;
            Status = DomainExtensionStatus.Disabled;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the status.
        /// </summary>
        /// <value>
        ///  The status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected DomainExtensionStatus Status { get; set; }

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
        public bool OnSessionCompleted(Guid sessionId)
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
                        DebugContract.Requires(Status == DomainExtensionStatus.ExtensionEnabled);
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
        protected virtual DomainExtensionStatus GetStatusAfterUnload()
        {
            return DomainExtensionStatus.Enabled;
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
        public virtual bool Unload(List<Guid> activeSessions, T extension)
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
                    PendingUnloadSessions = new List<Guid>(activeSessions);
                    return false;
                }

                OnUnload();
                return true;
            }
            return false;
        }
    }
}