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

namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for graph path list.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IGraphPathList
    {
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
        ///  Gets the retrieve.
        /// </summary>
        /// <returns>
        ///  A GraphPath.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        GraphPath Retrieve();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Clears this instance to its blank/initial state.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Clear();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Inserts the given child paths.
        /// </summary>
        /// <param name="childPaths">
        ///  The child paths.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Insert(IEnumerable<GraphPath> childPaths);
    }
}