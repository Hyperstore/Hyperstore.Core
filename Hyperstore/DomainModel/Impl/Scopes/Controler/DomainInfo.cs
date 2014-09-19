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