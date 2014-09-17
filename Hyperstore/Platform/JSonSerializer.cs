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
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Hyperstore.Modeling.Platform
{
    internal class JSonSerializer : Hyperstore.Modeling.IJsonSerializer
    {
        public string Serialize(object data)
        {
            if (data == null)
                return null;

            if (data is string)
                return (string)data;

            if (data is IModelElement)
                return Hyperstore.Modeling.Serialization.JSonSerializer.Serialize(data as IModelElement);

            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(data.GetType());
                serializer.WriteObject(stream, data);
                stream.Position = 0;
                using (StreamReader _Reader = new StreamReader(stream))
                { return _Reader.ReadToEnd(); }
            }
        }

        public T Deserialize<T>(string data)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)data;

            return (T)Deserialize(typeof(T), data, default(T));
        }

        public object Deserialize(Type implementationType, string data, object defaultValue)
        {
            Contract.Requires(implementationType, "implementationType");

            if (data == null)
                return defaultValue;

            if (implementationType == typeof(string))
                return data;

            var _bytes = Encoding.Unicode.GetBytes(data);
            using (MemoryStream _Stream = new MemoryStream(_bytes))
            {
                var serializer = new DataContractJsonSerializer(implementationType);
                return serializer.ReadObject(_Stream);
            }
        }
    }
}
