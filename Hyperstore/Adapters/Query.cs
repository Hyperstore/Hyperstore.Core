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
 
namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A query.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class Query
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string DomainModel { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the skip.
        /// </summary>
        /// <value>
        ///  The skip.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Skip { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the type of the node.
        /// </summary>
        /// <value>
        ///  The type of the node.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public NodeType NodeType { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the single.
        /// </summary>
        /// <value>
        ///  The identifier of the single.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SingleId { get; set; }
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema
        /// </summary>
        /// <value>
        ///  The meta class.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement Schema { get; set; }
    }
}