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
        /// <param name="serializer">
        ///  (Optional) the serializer.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        string Serialize(object value, Hyperstore.Modeling.IJsonSerializer serializer=null);

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