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

using System.Collections.Generic;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling.Adapters
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Encapsulates the result of a query node.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public class QueryNodeResult
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="props">
        ///  The properties.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public QueryNodeResult(IGraphNode node, ISchemaElement schema, Dictionary<ISchemaProperty, PropertyValue> props)
        {
            Contract.Requires(node != null, "node");
            Contract.Requires(schema != null, "schema");
            Node = node;
            SchemaInfo = schema;
            Properties = props;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets information describing the schema.
        /// </summary>
        /// <value>
        ///  Information describing the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement SchemaInfo { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the node.
        /// </summary>
        /// <value>
        ///  The node.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IGraphNode Node { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Associated properties of the current node. Can be null.
        /// </summary>
        /// <value>
        ///  The properties or null.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDictionary<ISchemaProperty, PropertyValue> Properties { get; private set; }
    }
}