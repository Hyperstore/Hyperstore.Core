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