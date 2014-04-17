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

namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A graph breadth first traverser.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Traversal.GraphPathTraverser"/>
    ///-------------------------------------------------------------------------------------------------
    public class GraphBreadthFirstTraverser : GraphPathTraverser
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
            return new PathQueue();
        }

        private class PathQueue : IGraphPathList
        {
            private readonly Queue<GraphPath> _queue = new Queue<GraphPath>();

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
                return _queue.Dequeue();
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
                get { return _queue.Count == 0; }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Clears this instance to its blank/initial state.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Clear()
            {
                _queue.Clear();
            }

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

                foreach (var path in childPaths)
                {
                    _queue.Enqueue(path);
                }
            }
        }
    }
}