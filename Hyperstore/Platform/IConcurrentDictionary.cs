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
