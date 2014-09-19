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