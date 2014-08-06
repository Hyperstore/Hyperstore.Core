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

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for cache adapter.
    /// </summary>
    /// <seealso cref="T:IDomainService"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ICacheAdapter : IDomainService
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IDomainModel DomainModel { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates an entity.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaEntity">
        ///  The schema entity.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IGraphNode CreateEntity(Identity id, ISchemaEntity schemaEntity);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="startId">
        ///  The start identifier.
        /// </param>
        /// <param name="startSchema">
        ///  The start schema.
        /// </param>
        /// <param name="endId">
        ///  The end identifier.
        /// </param>
        /// <param name="endSchema">
        ///  The end schema.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IGraphNode CreateRelationship(Identity id, ISchemaRelationship schemaRelationship, Identity startId, ISchemaElement startSchema, Identity endId, ISchemaElement endSchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the edges in this collection.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <param name="direction">
        ///  The direction.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="localOnly">
        ///  true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the edges in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IGraphNode> GetEdges(Identity id, ISchemaElement schemaElement, Direction direction, ISchemaRelationship schemaRelationship, bool localOnly);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets graph node.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <param name="localOnly">
        ///  true to local only.
        /// </param>
        /// <returns>
        ///  The graph node.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IGraphNode GetGraphNode(Identity id, ISchemaElement schemaElement, bool localOnly);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the graph nodes in this collection.
        /// </summary>
        /// <param name="elementType">
        ///  Type of the element.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <param name="localOnly">
        ///  true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the graph nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IGraphNode> GetGraphNodes(NodeType elementType, ISchemaElement schemaElement, bool localOnly);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <param name="ownerId">
        ///  The identifier that owns this item.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <param name="schemaProperty">
        ///  The schema property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue GetPropertyValue(Identity ownerId, ISchemaElement schemaElement, ISchemaProperty schemaProperty);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the entity.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <param name="schemaEntity">
        ///  The schema entity.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RemoveEntity(IGraphNode node, ISchemaEntity schemaEntity);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RemoveRelationship(IGraphNode node, ISchemaRelationship schemaRelationship);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value.
        /// </summary>
        /// <param name="mel">
        ///  The mel.
        /// </param>
        /// <param name="schemaProperty">
        ///  The schema property.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        /// <returns>
        ///  A PropertyValue.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue SetPropertyValue(IModelElement mel, ISchemaProperty schemaProperty, object value, long? version);
    }
}