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

using Hyperstore.Modeling.HyperGraph;
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
        public NodeInfo Node { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets from edge.
        /// </summary>
        /// <value>
        ///  from edge.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public EdgeInfo FromEdge { get; set; }

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