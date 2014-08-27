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
        public ExtensionInfo(T extension, List<Guid> activeSessions)
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
        public T GetDomainModel(Guid? sessionId)
        {
            //DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, " >>> GetDomainModel extension for session {0} - ALT is {1}", sessionId, DomainModel == null ? "null" : "not null");
            //if (PendingUnloadSessions != null)
            //    DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "      -- unload : {0}", String.Join(",", PendingUnloadSessions));
            //if (PendingLoadSessions != null)
            //    DomainModel.Store.Trace.WriteTrace(TraceCategory.DomainControler, "      -- load : {0}", String.Join(",", PendingLoadSessions));

            if (DomainModel != null
                && ( Status == ScopeStatus.ScopeEnabled) && (PendingUnloadSessions == null || sessionId == null || PendingUnloadSessions.Contains(sessionId.Value))
                && (PendingLoadSessions == null || sessionId == null || !PendingLoadSessions.Contains(sessionId.Value) || (Session.Current.Mode & SessionMode.LoadingSchema) == SessionMode.LoadingSchema)) // TODO ou Loading ???
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