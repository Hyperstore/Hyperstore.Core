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

using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.HyperGraph.Index
{
    internal class IndexWrapper : IIndex
    {
        private readonly IIndex _index;
        private readonly IHyperstore _store;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="store">
        ///  The store.
        /// </param>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public IndexWrapper(IHyperstore store, IIndex index)
        {
            DebugContract.Requires(store);
            DebugContract.Requires(index);
            _store = store;
            _index = index;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the specified key.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Identity Get(object key)
        {
            using (EnsuresRunInSession())
            {
                return _index.Get(key);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all.
        /// </summary>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<Identity> GetAll(int skip = 0)
        {
            using (EnsuresRunInSession())
            {
                return _index.GetAll(skip);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<Identity> GetAll(object key, int skip = 0)
        {
            using (EnsuresRunInSession())
            {
                return _index.GetAll(key, skip);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [is unique].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [is unique]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsUnique
        {
            get { return _index.IsUnique; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name.
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Name
        {
            get { return _index.Name; }
        }

        private ISession EnsuresRunInSession()
        {
            if (Session.Current != null)
                return null;

            return _store.BeginSession(new SessionConfiguration
                                       {
                                               Readonly = true
                                       });
        }
    }
}