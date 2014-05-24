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
using System.Threading.Tasks;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Adapters;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for query graph adapter.
    /// </summary>
    /// <seealso cref="T:IDomainService"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface IQueryGraphAdapter : IDomainService
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the associated domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IDomainModel DomainModel { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Read a node (or edge) from the graph.
        /// </summary>
        /// <param name="id">
        ///  The node id.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema of the element.
        /// </param>
        /// <returns>
        ///  A QueryNodeResult containing the node with this properties
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        QueryNodeResult GetNode(Identity id, ISchemaElement schemaElement);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Load.
        /// </summary>
        /// <param name="query">
        ///  A query in native query language.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process load nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<QueryNodeResult> LoadNodes(Query query);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the edges.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship of the element and all its derivated
        /// </param>
        /// <param name="includeProperties">
        ///  if set to <c>true</c> [include properties].
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the edges in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<QueryNodeResult> GetEdges(Identity id, Direction direction, ISchemaRelationship schemaRelationship, bool includeProperties);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the nodes.
        /// </summary>
        /// <param name="elementType">
        ///  Type of the element.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema container.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<QueryNodeResult> GetNodes(NodeType elementType, ISchemaElement schemaElement);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property.
        /// </summary>
        /// <param name="ownerId">
        ///  The owner id.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema container.
        /// </param>
        /// <param name="schemaProperty">
        ///  The schema property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue GetPropertyValue(Identity ownerId, ISchemaElement schemaElement, ISchemaProperty schemaProperty);
    }
}