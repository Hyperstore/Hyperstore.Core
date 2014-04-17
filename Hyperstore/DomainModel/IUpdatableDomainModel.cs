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
        ///  Gets the model element resolver.
        /// </summary>
        /// <value>
        ///  The model element resolver.
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
        /// <param name="localOnly">
        ///  (Optional)
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool RemoveRelationship(Identity id, ISchemaRelationship relationshipSchema, bool throwExceptionIfNotExists, bool localOnly = true);

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
        /// <param name="localOnly">
        ///  (Optional) if set to <c>true</c> [local only].
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool RemoveEntity(Identity id, ISchemaEntity entitySchema, bool throwExceptionIfNotExists, bool localOnly = true);

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