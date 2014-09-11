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

using System;
namespace Hyperstore.Modeling.Metadata.Primitives
{
    internal class StringPrimitive : PrimitiveMetaValue
    {
        protected StringPrimitive()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public StringPrimitive(ISchema domainModel)
            : base(domainModel, typeof(string))
        {
            DebugContract.Requires(domainModel, "domainModel");
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
            DebugContract.Requires(ctx);
            var str = ctx.Value as string;
            if (str == null || str.Length == 0)
                return null;

            if (str[0] == '"')
                return str.Substring(1, str.Length - 2);

            return str;
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
            var str = data as string;
            if (str == null)
                return null;

            return String.Format("\"{0}\"", str.Replace("\"", @"\"""));
        }
    }
}