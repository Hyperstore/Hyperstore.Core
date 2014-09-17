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
using System.Globalization;

#endregion

namespace Hyperstore.Modeling.Metadata.Primitives
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An int 32 primitive.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.Primitives.PrimitiveMetaValue"/>
    ///-------------------------------------------------------------------------------------------------
    public sealed class UInt32Primitive : PrimitiveMetaValue
    {
        #pragma warning disable 0628 // Hyperstore deserialization need a protected parameterless constructeur

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected UInt32Primitive()
        {
        }
        #pragma warning restore 0628


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        internal UInt32Primitive(ISchema domainModel)
            : base(domainModel, typeof(UInt32))
        {
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

        private static object DeserializeValue(SerializationContext ctx)
        {
            DebugContract.Requires(ctx);

            if (ctx.Value == null)
                return null;
            if (ctx.Value is UInt32)
                return ctx.Value;
            return UInt32.Parse((string)ctx.Value, CultureInfo.InvariantCulture);
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
            if (data == null)
                return null;
            return Convert.ToUInt32(data)
                    .ToString(CultureInfo.InvariantCulture);
        }
    }
}