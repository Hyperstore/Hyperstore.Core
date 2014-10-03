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
    internal interface IDomainInfos<T> where T : class,IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Activates the given domain.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Activate(T domain);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets domain model.
        /// </summary>
        /// <param name="sessionId">
        ///  Identifier for the session.
        /// </param>
        /// <returns>
        ///  The domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T GetDomainModel(int sessionId);

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
        bool Unload(List<int> activeSessions, T extension);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the session completed action.
        /// </summary>
        /// <param name="guid">
        ///  Unique identifier.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool OnSessionCompleted(int guid);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'name' is extension name exists.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  true if extension name exists, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsExtensionNameExists(string name);
    }
}