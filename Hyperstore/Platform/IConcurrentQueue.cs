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
    ///  Interface for concurrent queue.
    /// </summary>
    /// <typeparam name="TValue">
    ///  Type of the value.
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    public interface IConcurrentQueue<TValue>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an object onto the end of this queue.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Enqueue(TValue value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///  true if this instance is empty, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsEmpty { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Attempts to dequeue from the given data.
        /// </summary>
        /// <param name="v">
        ///  [out] The out TValue to process.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool TryDequeue(out TValue v);
    }
}
