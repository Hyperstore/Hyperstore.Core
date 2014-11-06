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

namespace Hyperstore.Modeling.Metadata.Primitives
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A primitive meta value.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.Primitives.PrimitiveMetaEntity"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaValueObject"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class PrimitiveMetaValue : PrimitiveMetaEntity, ISchemaValueObject
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected PrimitiveMetaValue()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="schema">
        ///  The domain model.
        /// </param>
        /// <param name="implementedType">
        ///  Type of the implemented.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected PrimitiveMetaValue(PrimitivesSchema schema, Type implementedType)
            : base(schema, implementedType, schema.SchemaValueObjectSchema)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  override this instance to the given stream.
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
            DebugContract.Requires(ctx);

            if (ctx.Value == null)
                return this.DefaultValue;

            if (ctx.Value.GetType() == this.ImplementedType)
                return ctx.Value;

            try
            {
                return Convert.ChangeType(ctx.Value, this.ImplementedType);
            }
            catch
            {
                throw new InvalidCastException(String.Format("Unable to cast property value for element {0} - Expected type {1}, current is {0}", ctx.Id, this.ImplementedType.FullName, ctx.Value.GetType().FullName));
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  override this instance to the given stream.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override object Serialize(object data)
        {
            return data;
        }

    }
}