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
    ///  Interface for graph path traverser.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IGraphPathTraverser
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes this instance.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Initialize(IGraphTraversalConfiguration query);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates the items in this collection that meet given criteria.
        /// </summary>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the matched items.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphPath> Traverse(IModelElement start);
    }
}