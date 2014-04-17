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