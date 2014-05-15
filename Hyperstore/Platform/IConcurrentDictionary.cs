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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for concurrent dictionary.
    /// </summary>
    /// <typeparam name="TKey">
    ///  Type of the key.
    /// </typeparam>
    /// <typeparam name="TValue">
    ///  Type of the value.
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    public interface IConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the entry to access.
        /// </param>
        /// <returns>
        ///  The indexed item.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TValue this[TKey index] { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Attempts to add from the given data.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  [out] The value.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool TryAdd(TKey key, TValue value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or add.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  [out] The value.
        /// </param>
        /// <returns>
        ///  The or add.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TValue GetOrAdd(TKey key, TValue value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the values.
        /// </summary>
        /// <value>
        ///  The values.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<TValue> Values { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Attempts to get value from the given data.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  [out] The value.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool TryGetValue(TKey key, out TValue value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Try remove.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  [out] The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        bool TryRemove(TKey key, out TValue value);
    }
}
