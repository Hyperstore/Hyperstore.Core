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