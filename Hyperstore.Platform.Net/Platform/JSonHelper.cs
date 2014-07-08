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

using Hyperstore.Modeling;
using Hyperstore.Modeling.Platform;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;


namespace Hyperstore.Modeling.Platform.Net
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A son helper.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class JSonSerializer : IObjectSerializer
    {
        private readonly JavaScriptSerializer _serializer;

        internal JSonSerializer()
        {
            _serializer = new JavaScriptSerializer();
            _serializer.RegisterConverters(new JavaScriptConverter[] { new IdentityConverter() });
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public string Serialize(object data)
        {
            if (data == null)
                return null;

            if (data is string)
                return (string)data;

            return _serializer.Serialize(data);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A T.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T Deserialize<T>(string data)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)data;
            return (T)_serializer.Deserialize(data, typeof(T));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="implementationType">
        ///  Type of the implementation.
        /// </param>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <param name="defaultValue">
        ///  The default value.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public object Deserialize(Type implementationType, string data, object defaultValue)
        {
            Contract.Requires(implementationType, "implementationType");

            if (data == null)
                return defaultValue;

            if (implementationType == typeof(string))
                return data;

            return _serializer.Deserialize(data, implementationType);
        }

        private class IdentityConverter : JavaScriptConverter
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  When overridden in a derived class, gets a collection of the supported types.
            /// </summary>
            /// <value>
            ///  An object that implements <see cref="T:System.Collections.Generic.IEnumerable`1" /> that
            ///  represents the types supported by the converter.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public override IEnumerable<Type> SupportedTypes
            {
                get { yield return typeof(Identity); }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  When overridden in a derived class, converts the provided dictionary into an object of the
            ///  specified type.
            /// </summary>
            /// <param name="dictionary">
            ///  An <see cref="T:System.Collections.Generic.IDictionary`2" /> instance of property data stored
            ///  as name/value pairs.
            /// </param>
            /// <param name="type">
            ///  The type of the resulting object.
            /// </param>
            /// <param name="serializer">
            ///  The <see cref="T:System.Web.Script.Serialization.JavaScriptSerializer" /> instance.
            /// </param>
            /// <returns>
            ///  The deserialized object.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                DebugContract.Requires(dictionary);
                DebugContract.Requires(type);
                DebugContract.Requires(serializer);

                return new Identity(dictionary["DmId"].ToString(), (string)dictionary["Key"]);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  When overridden in a derived class, builds a dictionary of name/value pairs.
            /// </summary>
            /// <param name="obj">
            ///  The object to serialize.
            /// </param>
            /// <param name="serializer">
            ///  The object that is responsible for the serialization.
            /// </param>
            /// <returns>
            ///  An object that contains key/value pairs that represent the object’s data.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                DebugContract.Requires(serializer);

                var pairs = new Dictionary<string, object>();
                var id = (Identity)obj;
                pairs["DmId"] = id.DomainModelName;
                pairs["Key"] = id.Key;
                return pairs;
            }
        }
    }
}

