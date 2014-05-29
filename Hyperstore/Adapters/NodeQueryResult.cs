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
    public class QueryNodeResult : IGraphNode 
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
        public QueryNodeResult(NodeType nodeType, Identity id, Identity startId, Identity startSchemaId, Identity endId, Identity endSchemaId, ISchemaElement schema, Dictionary<ISchemaProperty, PropertyValue> props)
        {
            Contract.Requires(id != null, "id");
            Contract.Requires(schema != null, "schema");
            NodeType = nodeType;
            Id = id;
            StartId = startId;
            StartSchemaId = startSchemaId;
            EndId = endId;
            EndSchemaId = endSchemaId;
            SchemaInfo = schema;
            Properties = props;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the type of the node.
        /// </summary>
        /// <value>
        ///  The type of the node.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public NodeType NodeType { get; private set; }

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
        ///  Gets the id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get { return SchemaInfo.Id; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartSchemaId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndSchemaId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start.
        /// </summary>
        /// <value>
        ///  The identifier of the start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end.
        /// </summary>
        /// <value>
        ///  The identifier of the end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndId { get; private set; }

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