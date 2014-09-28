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

using Hyperstore.Modeling.HyperGraph;
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
    public interface ITraversalQuery
    {
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
        ///  Gets or sets the evaluator.
        /// </summary>
        /// <value>
        ///  The evaluator.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ITraversalVisitor Evaluator { get; set; }

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
        ///  Gets the paths in this collection.
        /// </summary>
        /// <param name="node">
        ///  The start node.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the paths in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphPath> GetPaths(NodeInfo node);
    }
}