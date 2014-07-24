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
using Hyperstore.Modeling.Validations;
using Hyperstore.Modeling.Statistics;
using System;
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for schema.
    /// </summary>
    /// <seealso cref="T:IDomainModel"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ISchema : IDomainModel
    {        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets domain behaviors.
        /// </summary>
        /// <value>
        ///  The behavior.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        DomainBehavior Behavior { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads schema extension.
        /// </summary>
        /// <param name="definition">
        ///  The definition.
        /// </param>
        /// <param name="mode">
        ///  (Optional) the mode.
        /// </param>
        /// <returns>
        ///  The schema extension.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task<ISchema> LoadSchemaExtension(ISchemaDefinition definition, SchemaConstraintExtensionMode mode = SchemaConstraintExtensionMode.Inherit);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes the specified definition.
        /// </summary>
        /// <param name="definition">
        ///  The definition.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        Task Initialize(ISchemaDefinition definition);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the constraints.
        /// </summary>
        /// <value>
        ///  The constraints.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IConstraintsManager Constraints { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="schemaElementId">
        ///  The schema element id.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional)
        ///  if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaInfo GetSchemaInfo(Identity schemaElementId, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional)
        ///  if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaInfo GetSchemaInfo(string name, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity GetSchemaEntity<T>(bool throwErrorIfNotExists = true) where T : IModelEntity;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <param name="metaClassId">
        ///  The meta class id.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity GetSchemaEntity(Identity metaClassId, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity GetSchemaEntity(string name, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement GetSchemaElement(string name, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement GetSchemaElement(Identity id, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadatas.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema infos in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaInfo> GetSchemaInfos();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema elements in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema elements in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaElement> GetSchemaElements();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entities in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema entities in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaEntity> GetSchemaEntities();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationships.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema relationships in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaRelationship> GetSchemaRelationships();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="metaClassId">
        ///  The meta class id.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional)
        ///  if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship GetSchemaRelationship(Identity metaClassId, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional)
        ///  if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship GetSchemaRelationship(string name, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional)
        ///  if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship GetSchemaRelationship<T>(bool throwErrorIfNotExists = true) where T : IModelRelationship;
    }
}