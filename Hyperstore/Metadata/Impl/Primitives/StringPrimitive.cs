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
    }
}