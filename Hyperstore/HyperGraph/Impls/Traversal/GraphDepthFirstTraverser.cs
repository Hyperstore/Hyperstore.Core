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
    ///  A graph depth first traverser.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Traversal.GraphPathTraverser"/>
    ///-------------------------------------------------------------------------------------------------
    public class GraphDepthFirstTraverser : GraphPathTraverser
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates path container.
        /// </summary>
        /// <returns>
        ///  The new path container.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override IGraphPathList CreatePathContainer()
        {
            return new PathStack();
        }

        private class PathStack : IGraphPathList
        {
            private readonly Stack<GraphPath> _stack = new Stack<GraphPath>();

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Inserts the given child paths.
            /// </summary>
            /// <param name="childPaths">
            ///  The child paths.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void Insert(IEnumerable<GraphPath> childPaths)
            {
                DebugContract.Requires(childPaths);

                foreach (var path in childPaths.Reverse())
                {
                    _stack.Push(path);
                }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the retrieve.
            /// </summary>
            /// <returns>
            ///  A GraphPath.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public GraphPath Retrieve()
            {
                return _stack.Pop();
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets a value indicating whether this instance is empty.
            /// </summary>
            /// <value>
            ///  true if this instance is empty, false if not.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public bool IsEmpty
            {
                get { return _stack.Count == 0; }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Clears this instance to its blank/initial state.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Clear()
            {
                _stack.Clear();
            }
        }
    }
}