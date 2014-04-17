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