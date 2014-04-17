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
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for schema information.
    /// </summary>
    /// <seealso cref="T:IModelElement"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ISchemaInfo : IModelElement
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the schema. </summary>
        /// <value> The schema. </value>
        ///-------------------------------------------------------------------------------------------------
        ISchema Schema { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name.
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string Name { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the implemented type.
        /// </summary>
        /// <value>
        ///  The type of the implemented.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Type ImplementedType { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default value.
        /// </summary>
        /// <value>
        ///  The default value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        object DefaultValue { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is primitive.
        /// </summary>
        /// <value>
        ///  true if this instance is primitive, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsPrimitive { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the super class.
        /// </summary>
        /// <value>
        ///  The super class.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement SuperClass { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Deserializes an element from the specified context.
        /// </summary>
        /// <param name="ctx">
        ///  Serialization context.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        object Deserialize(SerializationContext ctx);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serializes the specified value.
        /// </summary>
        /// <param name="value">
        ///  Value to serialized.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        string Serialize(object value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the specified meta class is A.
        /// </summary>
        /// <param name="metaClass">
        ///  The meta class.
        /// </param>
        /// <returns>
        ///  <c>true</c> if the specified meta class is A; otherwise, <c>false</c>.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsA(ISchemaInfo metaClass);
    }
}