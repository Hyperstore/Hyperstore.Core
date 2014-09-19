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
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for schema element.
    /// </summary>
    /// <seealso cref="T:ISchemaInfo"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ISchemaElement : ISchemaInfo
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a property by name.
        /// </summary>
        /// <param name="name">
        ///  The property name.
        /// </param>
        /// <returns>
        ///  The property.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaProperty GetProperty(string name);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a property.
        /// </summary>
        /// <param name="name">
        ///  The property name.
        /// </param>
        /// <param name="property">
        ///  The property definition.
        /// </param>
        /// <param name="defaultValue">
        ///  (Optional) The default value.
        /// </param>
        /// <param name="kind">
        ///  (Optional) the kind.
        /// </param>
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaProperty DefineProperty(string name, ISchemaValueObject property, object defaultValue = null, PropertyKind kind = PropertyKind.Normal);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a property.
        /// </summary>
        /// <param name="property">
        ///  The property definition.
        /// </param>
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaProperty DefineProperty(ISchemaProperty property);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a property.
        /// </summary>
        /// <typeparam name="T">
        ///  Type of the property.
        /// </typeparam>
        /// <param name="name">
        ///  The property name.
        /// </param>
        /// <param name="defaultValue">
        ///  (Optional) The default value.
        /// </param>
        /// <param name="kind">
        ///  (Optional) the kind.
        /// </param>
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaProperty DefineProperty<T>(string name, object defaultValue = null, PropertyKind kind = PropertyKind.Normal);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the properties.
        /// </summary>
        /// <param name="recursive">
        ///  if set to <c>true</c> [recursive].
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the properties in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaProperty> GetProperties(bool recursive);
    }
}