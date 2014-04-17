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
    ///  A graph position.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class GraphPosition
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the node.
        /// </summary>
        /// <value>
        ///  The node.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement Node { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets from edge.
        /// </summary>
        /// <value>
        ///  from edge.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship FromEdge { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets a value indicating whether this instance is start position.
        /// </summary>
        /// <value>
        ///  true if this instance is start position, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsStartPosition { get; set; }
    }
}