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
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Adapters
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for graph adapter.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IGraphAdapter
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates load nodes in this collection.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process load nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphPropertiesNode> LoadNodes(Query query);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for adpater implementing a lazy loading mechanism
    /// </summary>
    /// <seealso cref="T:IGraphAdapter"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ISupportsLazyLoading : IGraphAdapter
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates lazy loading nodes in this collection.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process lazy loading nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphPropertiesNode> LazyLoadingNodes(Query query);

    }
}
