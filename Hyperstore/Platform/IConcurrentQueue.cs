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
