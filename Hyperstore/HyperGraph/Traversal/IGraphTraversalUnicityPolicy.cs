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
 
namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Traversal path unicity policy.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IGraphTraversalUnicityPolicy
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns if a path has been visited.
        /// </summary>
        /// <param name="path">
        ///  The current path.
        /// </param>
        /// <returns>
        ///  True is the path has been visited.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsVisited(GraphPath path);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when a path is visited.
        /// </summary>
        /// <param name="path">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void MarkVisited(GraphPath path);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Reset when a new traversal query begins.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Reset();
    }
}