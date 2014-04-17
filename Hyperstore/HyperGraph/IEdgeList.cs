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
 
namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for edge list.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEdgeList
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of. 
        /// </summary>
        /// <value>
        ///  The count.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        int Count { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds node.
        /// </summary>
        /// <param name="node">
        ///  The node to add.
        /// </param>
        /// <returns>
        ///  An EdgeList.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        EdgeList Add(EdgeInfo node);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the by key described by ID.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  An EdgeList.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        EdgeList RemoveByKey(Identity id);
    }
}