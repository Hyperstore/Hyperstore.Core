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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Hyperstore.Modeling.Serialization
{
    class XmlWriter : Hyperstore.Modeling.Serialization.ISerializerWriter
    {
        private Stack<List<XElement>> _scopes = new Stack<List<XElement>>();
        private SerializationOptions _options;

        public XmlWriter(SerializationOptions options)
        {
            this._options = options;
            NewScope();
        }

        private bool HasOption(SerializationOptions option)
        {
            return (_options & option) == option;
        }

        public void NewScope()
        {
            _scopes.Push(new List<XElement>());
        }

        public void SaveTo(Stream stream, IDomainModel domain)
        {
            var writer = global::System.Xml.XmlWriter.Create(stream);

            var list = _scopes.Pop();
            var root = new XElement("domain", new XAttribute("name", domain.Name));
            if (domain.ExtensionName != null)
                root.Add(new XAttribute("extension", domain.ExtensionName));
            if (list.Count > 0)
                root.Add(list);
            root.WriteTo(writer);
            writer.Flush();
        }

        public void ReduceScope(string tag)
        {
            var list = _scopes.Pop();

            if (list.Count > 0)
            {
                var current = new XElement(tag, list);
                _scopes.Peek().Add(current);
            }
        }

        public void PushElement(string name, string id, string schemaId, string startId = null, string startSchemaId = null, string endId = null, string endSchemaId = null)
        {

            var current = new XElement(name, new XAttribute("id", id), new XAttribute("schema", schemaId));

            if (startId != null)
            {
                current.Add(
                             new XAttribute("start", startId), new XAttribute("startSchema", startSchemaId),
                             new XAttribute("end", endId), new XAttribute("endSchema", endSchemaId));
            }

            var list = _scopes.Pop();
            if (list.Count > 0)
            {
                current.Add(new XElement("properties", list));
            }

            _scopes.Peek().Add(current);
        }

        public void PushProperty(string tag, string name, object value)
        {
            var elem = new XElement(tag, new XAttribute("name", name), new XElement("value", value));
            _scopes.Peek().Add(elem);
        }

        public void SaveSchema(Dictionary<Identity, string> monikers)
        {
            if (HasOption(SerializationOptions.CompressSchema))
            {
                List<XElement> list = null;
                NewScope();

                foreach (var kv in monikers)
                {
                    var current = new XElement("add", new XAttribute("id", kv.Value), new XAttribute("name", kv.Key));
                    _scopes.Peek().Add(current);
                }

                list = _scopes.Pop();
                if (list.Count > 0)
                {
                    var current = new XElement("schemas", list);
                    _scopes.Peek().Insert(0, current);
                }
            }
        }
    }
}
