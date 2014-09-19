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
using System.Linq;

#endregion

namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A global node unicity.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Traversal.IGraphTraversalUnicityPolicy"/>
    ///-------------------------------------------------------------------------------------------------
    public class GlobalNodeUnicity : IGraphTraversalUnicityPolicy
    {
        private readonly HashSet<GraphPath> _visited = new HashSet<GraphPath>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns if a path has been visited.
        /// </summary>
        /// <param name="path">
        ///  Full pathname of the file.
        /// </param>
        /// <returns>
        ///  True is the path has been visited.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsVisited(GraphPath path)
        {
            DebugContract.Requires(path);

            return _visited.Contains(path);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when a path is visited.
        /// </summary>
        /// <param name="path">
        ///  Full pathname of the file.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void MarkVisited(GraphPath path)
        {
            DebugContract.Requires(path);

            _visited.Add(path);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Reset when a new traversal query begins.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Reset()
        {
            _visited.Clear();
        }
    }
}