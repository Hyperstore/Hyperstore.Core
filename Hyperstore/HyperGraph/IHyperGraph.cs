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
        GraphNode CreateEntity(Identity id, ISchemaEntity schemaEntity);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <returns>
        ///  the schema of the removed entity or null
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity RemoveEntity(Identity id, bool throwExceptionIfNotExists);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelEntity GetEntity(Identity id);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the relationship.
        /// </summary>
        /// <param name="endSchema">
        ///  The end schema.
        /// </param>
        ///
        /// <param name="startSchema">
        ///  .
        /// </param>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="startId">
        ///  The start.
        /// </param>
        /// <param name="endId">
        ///  The end.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        GraphNode CreateRelationship(Identity id, ISchemaRelationship schemaRelationship, Identity startId, Identity endId);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelRelationship GetRelationship(Identity id);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <returns>
        ///  the schema of the removed relationship or null
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship RemoveRelationship(Identity id, bool throwExceptionIfNotExists);

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
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelElement GetElement(Identity id);

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
        IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship schemaRelationship, IModelElement start, IModelElement end, int skip = 0);

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
        IEnumerable<T> GetRelationships<T>(ISchemaRelationship schemaRelationship, IModelElement start, IModelElement end, int skip = 0) where T : IModelRelationship;

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
        IEnumerable<IModelEntity> GetEntities(ISchemaEntity schemaEntity, int skip = 0);

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
        IEnumerable<IModelElement> GetElements(ISchemaElement schemaElement, int skip = 0);

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
        IEnumerable<T> GetEntities<T>(ISchemaEntity schemaEntity, int skip = 0) where T : IModelEntity;

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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads the nodes.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <param name="option">
        ///  The option.
        /// </param>
        /// <param name="adapter">
        ///  The adapter.
        /// </param>
        /// <param name="lazyLoading">
        ///  true to lazy loading.
        /// </param>
        /// <returns>
        ///  The nodes.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task<int> LoadNodes(Query query, MergeOption option, Hyperstore.Modeling.Adapters.IGraphAdapter adapter, bool lazyLoading);
    }
}