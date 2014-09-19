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

using System;

#endregion

namespace Hyperstore.Modeling
{
    /// <summary>
    /// </summary>
    internal interface IUpdatableDomainModel : IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the model element services.
        /// </summary>
        /// <value>
        ///  The model element services.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IModelElementFactory ModelElementFactory { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="entitySchema">
        ///  The entity Schema.
        /// </param>
        /// <param name="instance">
        ///  (Optional) The instance.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelElement CreateEntity(Identity id, ISchemaEntity entitySchema, IModelEntity instance = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the relation ship.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="relationshipSchema">
        ///  The relationship Schema.
        /// </param>
        /// <param name="start">
        ///  .
        /// </param>
        /// <param name="endId">
        ///  .
        /// </param>
        /// <param name="endSchema">
        ///  .
        /// </param>
        /// <param name="relationship">
        ///  (Optional) The relationship.
        /// </param>
        /// <returns>
        ///  The new relation ship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelRelationship CreateRelationship(Identity id, ISchemaRelationship relationshipSchema, IModelElement start, Identity endId, ISchemaElement endSchema, IModelRelationship relationship = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="relationshipSchema">
        ///  The relationship Schema.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool RemoveRelationship(Identity id, ISchemaRelationship relationshipSchema, bool throwExceptionIfNotExists);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="entitySchema">
        ///  The entity schema.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  if set to <c>true</c> [throw exception if not exists].
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool RemoveEntity(Identity id, ISchemaEntity entitySchema, bool throwExceptionIfNotExists);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value.
        /// </summary>
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <param name="propertySchema">
        ///  The property schema.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="version">
        ///  (Optional)
        ///  The version.
        /// </param>
        /// <returns>
        ///  A Hyperstore.Modeling.HyperGraph.PropertyValue.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue SetPropertyValue(IModelElement owner, ISchemaProperty propertySchema, object value, long? version=null);
    }
}