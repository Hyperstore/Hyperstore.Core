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

using System;
using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for graph traversal configuration.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IGraphTraversalConfiguration
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the evaluator.
        /// </summary>
        /// <value>
        ///  The evaluator.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IGraphTraversalEvaluator Evaluator { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the path traverser.
        /// </summary>
        /// <value>
        ///  The path traverser.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IGraphPathTraverser PathTraverser { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the unicity policy.
        /// </summary>
        /// <value>
        ///  The unicity policy.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IGraphTraversalUnicityPolicy UnicityPolicy { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the incidences iterator.
        /// </summary>
        /// <value>
        ///  The incidences iterator.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        INodeIncidenceIterator IncidencesIterator { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IDomainModel DomainModel { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the paths in this collection.
        /// </summary>
        /// <param name="startNode">
        ///  The start node.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the paths in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphPath> GetPaths(IModelElement startNode);
    }
}