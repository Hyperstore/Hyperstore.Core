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

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for index.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IIndex
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [is unique].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [is unique]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsUnique { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name.
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string Name { get; }

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
        Identity Get(object key);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all.
        /// </summary>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<Identity> GetAll(int skip = 0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<Identity> GetAll(object key, int skip = 0);
    }
}