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
        ///  Get the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IDomainModel DomainModel { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity Id { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the metadata.
        /// </summary>
        /// <value>
        ///  The metadata.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement SchemaInfo { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the status.
        /// </summary>
        /// <value>
        ///  The status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ModelElementStatus Status { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the creation sequence.
        /// </summary>
        /// <value>
        ///  The sequence.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        int Sequence { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get value of a property. Returns a PropertyValue where version property can be 0 if there is no value for this property
        /// </summary>
        /// <param name="property">
        ///  The property description
        /// </param>
        /// <returns>
        ///  A PropertyValue class
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue GetPropertyValue(ISchemaProperty property);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get value of a property. Returns a PropertyValue where version property can be 0 if there is no
        ///  value for this property.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="propertyName">
        ///  The property name
        /// </param>
        /// <returns>
        ///  A PropertyValue class.
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
        ///  List of relationships
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship relationshipSchema = null, IModelElement end = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get all relationships outgoings from the current element
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="end">
        ///  (Optional) End element, if precised return only relationships between the current and the end element.
        /// </param>
        /// <returns>
        ///  List of relationships
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<T> GetRelationships<T>(IModelElement end = null) where T : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Remove this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Remove();
    }
}