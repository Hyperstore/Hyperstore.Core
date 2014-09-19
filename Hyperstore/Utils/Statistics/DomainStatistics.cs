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
 
namespace Hyperstore.Modeling.Statistics
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A domain statistics.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class DomainStatistics
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public DomainStatistics()
        {
            NumberOfEdges = new StatisticCounter("");
            NumberOfNodes = new StatisticCounter("");
            NumberOfTransactions = new StatisticCounter("");
            NodesCreated = new StatisticCounter("");
            NodesDeleted = new StatisticCounter("");
            RelationshipsCreated = new StatisticCounter("");
            RelationshipsDeleted = new StatisticCounter("");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of nodes.
        /// </summary>
        /// <value>
        ///  The total number of nodes.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter NumberOfNodes { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of edges.
        /// </summary>
        /// <value>
        ///  The total number of edges.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter NumberOfEdges { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of transactions.
        /// </summary>
        /// <value>
        ///  The total number of transactions.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter NumberOfTransactions { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the nodes created.
        /// </summary>
        /// <value>
        ///  The nodes created.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter NodesCreated { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships created.
        /// </summary>
        /// <value>
        ///  The relationships created.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter RelationshipsCreated { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the nodes deleted.
        /// </summary>
        /// <value>
        ///  The nodes deleted.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter NodesDeleted { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships deleted.
        /// </summary>
        /// <value>
        ///  The relationships deleted.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter RelationshipsDeleted { get; private set; }
    }
}