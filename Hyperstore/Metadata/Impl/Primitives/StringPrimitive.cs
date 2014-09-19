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
 
using System;
namespace Hyperstore.Modeling.Metadata.Primitives
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A string primitive.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.Primitives.PrimitiveMetaValue"/>
    ///-------------------------------------------------------------------------------------------------
    public sealed class StringPrimitive : PrimitiveMetaValue
    {
        #pragma warning disable 0628 // Hyperstore deserialization need a protected parameterless constructeur

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected StringPrimitive()
        {
        }
        #pragma warning restore 0628

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="schema">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        internal StringPrimitive(ISchema schema)
            : base(schema, typeof(string))
        {
            DebugContract.Requires(schema, "schema");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override object Deserialize(SerializationContext ctx)
        {
            return DeserializeValue(ctx);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Deserialize value.
        /// </summary>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static object DeserializeValue(SerializationContext ctx)
        {
            DebugContract.Requires(ctx);

            var str = ctx.Value as string;
            if (str == null || str.Length == 0)
                return null;

            if (str[0] == '"')
                return str.Substring(1, str.Length - 2).Replace(@"\""", "\"");

            return str.Replace(@"\""", "\"");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <param name="serializer">
        ///  The serializer.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string Serialize(object data, IJsonSerializer serializer)
        {
            return SerializeValue(data);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serialize value.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string SerializeValue(object data)
        {
            var str = data as string;
            if (str == null)
                return null;

            return String.Format("\"{0}\"", str.Replace("\"", @"\"""));
        }
    }
}