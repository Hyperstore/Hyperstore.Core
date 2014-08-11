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

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for hyper graph.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IHyperGraph
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
        ///  Adds the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
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
        ///  Removes the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="schemaEntity">
        ///  The meta class.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool RemoveEntity(Identity id, ISchemaEntity schemaEntity, bool throwExceptionIfNotExists);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="schemaEntity">
        ///  The schema Container.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelEntity GetEntity(Identity id, ISchemaEntity schemaEntity);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the relationship.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="startId">
        ///  The start.
        /// </param>
        /// <param name="startSchema">
        ///  .
        /// </param>
        /// <param name="endId">
        ///  The end.
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
        ///  Gets the relationship.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelRelationship GetRelationship(Identity id, ISchemaRelationship schemaRelationship);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool RemoveRelationship(Identity id, ISchemaRelationship schemaRelationship, bool throwExceptionIfNotExists);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the attribute.
        /// </summary>
        /// <param name="ownerId">
        ///  The owner id.
        /// </param>
        /// <param name="schemaProperty">
        ///  The schema property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue GetPropertyValue(Identity ownerId, ISchemaProperty schemaProperty);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element or relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema container.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelElement GetElement(Identity id, ISchemaElement schemaElement);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value.
        /// </summary>
        /// <param name="owner">
        ///  The owner.
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
        PropertyValue SetPropertyValue(IModelElement owner, ISchemaProperty schemaProperty, object value, long? version);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship schemaRelationship, IModelElement start, IModelElement end, int skip=0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<T> GetRelationships<T>(ISchemaRelationship schemaRelationship, IModelElement start, IModelElement end, int skip=0) where T : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <param name="schemaEntity">
        ///  The schema entity.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelEntity> GetEntities(ISchemaEntity schemaEntity, int skip=0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element or relationships.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema container.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelElement> GetElements(ISchemaElement schemaElement, int skip=0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="schemaEntity">
        ///  The schema entity.
        /// </param>
        /// <param name="skip">
        ///  The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<T> GetEntities<T>(ISchemaEntity schemaEntity, int skip=0) where T : IModelEntity;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'id' is deleted.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  true if deleted, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsDeleted(Identity id);
    }
}