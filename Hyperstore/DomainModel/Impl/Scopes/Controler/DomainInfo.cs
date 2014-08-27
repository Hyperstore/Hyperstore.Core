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

#endregion

namespace Hyperstore.Modeling.Scopes
{
    /// <summary>
    ///     Information sur un domaine et ses extensions
    /// </summary>
    internal class DomainInfo<T> : ScopeInfo<T>, IDomainInfos<T> where T:class, IDomainModel
    {
        private readonly bool _isSchema;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Le domaine principal.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainInfo(T domainModel) : base(domainModel)
        {
            _isSchema = domainModel is ISchema;
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
            if (DomainModel.InstanceId == domain.InstanceId)
            {
                if (Status == ScopeStatus.Disabled)
                    Status = ScopeStatus.Enabled;
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
            // Un schema est tjs actif
            if ((_isSchema || Status != ScopeStatus.Disabled) && (PendingUnloadSessions == null || sessionId == null || PendingUnloadSessions.Contains(sessionId.Value)))
            {
                return DomainModel;
            }
            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets status after unload.
        /// </summary>
        /// <returns>
        ///  The status after unload.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override ScopeStatus GetStatusAfterUnload()
        {
            return ScopeStatus.Disabled;
        }


        public bool IsExtensionNameExists(string name)
        {
            return false;
        }
    }
}