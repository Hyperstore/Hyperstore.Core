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

using System;
using System.Collections.Generic;
namespace Hyperstore.Modeling.Scopes
{
    enum ScopesSelector
    {
        Enabled,
        Loaded
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for model list.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:IEnumerable{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IModelList<T> where T : class, IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IHyperstore Store { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get a domain or schema.
        /// </summary>
        /// <param name="name">
        ///  The name to get.
        /// </param>
        /// <param name="session">
        ///  (Optional) the current session
        /// </param>
        /// <returns>
        ///  A domain or schema or null if not exists or not enabled in the session.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T Get(string name, ISession session = null);
    }

    interface IScopeManager<T> : IModelList<T>, IDisposable where T : class, global::Hyperstore.Modeling.IDomainModel
    {
        void EnableScope(T scope);
        T GetActiveScope(string name, int sessionId);
        void OnSessionCreated(global::Hyperstore.Modeling.ISession session, int sessionId = 0);
        void RegisterScope(T scope);
        void UnloadScope(T scope);
        System.Collections.Generic.IEnumerable<T> GetScopes(ScopesSelector selector, int sessionId = 0);
    }
}
