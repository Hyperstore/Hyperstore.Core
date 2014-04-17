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

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for model element.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface IModelElement
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
        ///  Gets the id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity Id { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <value>
        ///  The metadata.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement SchemaInfo { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the status.
        /// </summary>
        /// <value>
        ///  The status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ModelElementStatus Status { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the creation sequence.
        /// </summary>
        /// <value>
        ///  The sequence.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        int Sequence { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue GetPropertyValue(ISchemaProperty property);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property value.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T GetPropertyValue<T>(string propertyName);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <param name="relationshipSchema">
        ///  (Optional) The relationship schema.
        /// </param>
        /// <param name="end">
        ///  (Optional) The end.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship relationshipSchema = null, IModelElement end = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="end">
        ///  (Optional) The end.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<T> GetRelationships<T>(IModelElement end = null) where T : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Remove();
    }
}