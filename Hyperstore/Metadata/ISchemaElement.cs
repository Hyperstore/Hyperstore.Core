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
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaProperty DefineProperty(string name, ISchemaValueObject property, object defaultValue = null);

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
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaProperty DefineProperty<T>(string name, object defaultValue = null);

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