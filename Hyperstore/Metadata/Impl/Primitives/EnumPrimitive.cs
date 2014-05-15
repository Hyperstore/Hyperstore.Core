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

namespace Hyperstore.Modeling.Metadata.Primitives
{
    internal class EnumPrimitive<TEnum> : EnumPrimitive where TEnum : struct
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EnumPrimitive(ISchema schema)
            : base(schema, typeof(TEnum))
        {
        }

        protected EnumPrimitive()
        {
        }
    }

    internal class EnumPrimitive : SchemaValueObject
    {
        protected EnumPrimitive()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="tenum">
        ///  The tenum.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EnumPrimitive(ISchema schema, Type tenum)
            : base(schema, tenum)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serializes the specified data.
        /// </summary>
        /// <param name="mel">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override string Serialize(object mel)
        {
            return mel != null ? ((Enum)mel).ToString("F") : null;
        }

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
        protected override object Deserialize(SerializationContext ctx)
        {
            DebugContract.Requires(ctx);
            if (ctx.Value == null)
                return DefaultValue;

            if (ctx.Value.GetType() == ImplementedType)
                return ctx.Value;

            return Enum.Parse(ImplementedType, ctx.Value.ToString());
        }
    }
}