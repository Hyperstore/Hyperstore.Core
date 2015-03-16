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
        private SerializationOptions _options;
        private Stack<XElement> _scopes = new Stack<XElement>();
        private XElement _current;

        public XmlWriter(SerializationOptions options, IDomainModel domain)
        {
            this._options = options;

            var root = new XElement("domain", new XAttribute("name", domain.Name));
            if (domain.ExtensionName != null)
                root.Add(new XAttribute("extension", domain.ExtensionName));
            _scopes.Push(root);
        }

        private bool HasOption(SerializationOptions option)
        {
            return (_options & option) == option;
        }

        public void NewScope(string tag)
        {
            _scopes.Push(new XElement(tag));
        }

        public void ReduceScope()
        {
            var elem = _scopes.Pop();

            if (elem.HasElements || elem.HasAttributes)
            {
                _scopes.Peek().Add(elem);
            }
        }

        public void PushDeletedElement(string name, string id)
        {
            _current = new XElement(name, new XAttribute("id", id), new XAttribute("deleted", true));
            _scopes.Peek().Add(_current);
        }

        public void PushElement(string name, string id, string schemaId, string startId = null, string startSchemaId = null, string endId = null, string endSchemaId = null)
        {
            _current = new XElement(name, new XAttribute("id", id), new XAttribute("schema", schemaId));
            _scopes.Peek().Add(_current);
            if (startId != null)
            {
                _current.Add(
                             new XAttribute("start", startId), new XAttribute("startSchema", startSchemaId),
                             new XAttribute("end", endId), new XAttribute("endSchema", endSchemaId));
            }
        }

        public void PushProperty(string tag, string name, object value)
        {
            if( _current.Name != "properties")
            {
                var tmp = new XElement("properties");
                _current.Add(tmp);
                _current = tmp;
            }
            var elem = new XElement(tag, new XAttribute("name", name), new XElement("value", value));
            _current.Add(elem);
        }

        public void Save(Stream stream, IEnumerable<MonikerEntry> monikers)
        {
            var writer = global::System.Xml.XmlWriter.Create(stream);
            SaveSchema(monikers);
            var elem = _scopes.Pop();
            elem.WriteTo(writer);
            writer.Flush();
        }

        public void SaveSchema(IEnumerable<MonikerEntry> monikers)
        {
            if (HasOption(SerializationOptions.CompressSchema) && monikers.Any())
            {
                var current = new XElement("schemas");
                foreach (var schemas in monikers.GroupBy(s => s.SchemaName))
                {
                    var schema = new XElement("schema", new XAttribute("name", schemas.Key));
                    current.Add(schema);
                    foreach (var moniker in schemas)
                    {
                        schema.Add(new XElement("add", new XAttribute("id", moniker.Moniker), new XAttribute("name", moniker.Schema.Id.Key)));
                    }
                }
                _scopes.Peek().AddFirst(current);
            }
        }
    }
}
