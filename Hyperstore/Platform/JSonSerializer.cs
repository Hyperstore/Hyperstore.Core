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
