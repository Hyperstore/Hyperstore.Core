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
        private XElement _current;
        private Stack<List<XElement>> _scopes = new Stack<List<XElement>>();

        public XmlWriter()
        {
            NewScope();
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

        public void ReduceScope(string tag, string name = null, bool unshift = false)
        {
            var list = _scopes.Pop();

            if (list.Count > 0)
            {
                _current = new XElement(tag, list);
                if (!unshift)
                    _scopes.Peek().Add(_current);
                else
                    _scopes.Peek().Insert(0, _current);
                if (name != null)
                    _current.Add(new XAttribute("name", name));
            }
        }

        public void PushElement(string name, string id, string schemaId = null)
        {
            _current = new XElement(name, new XAttribute("id", id));
            var list = _scopes.Pop();
            if (list.Count > 0)
            {
                _current.Add(new XElement("properties", list));
            }
            _scopes.Peek().Add(_current);
            if (schemaId != null)
                _current.Add(new XAttribute("schema", schemaId));
        }

        public void PushElement(string name, string id, string startId, string startSchemaId, string endId, string endSchemaId, string schemaId = null)
        {
            _current = new XElement(name, new XAttribute("id", id),
                             new XAttribute("start", startId), new XAttribute("startSchema", startSchemaId),
                             new XAttribute("end", endId), new XAttribute("endSchema", endSchemaId));
            var list = _scopes.Pop();
            if (list.Count > 0)
            {
                _current.Add(new XElement("properties", list));
            }
            _scopes.Peek().Add(_current);
            if (schemaId != null)
                _current.Add(new XAttribute("schema", schemaId));
        }

        public void PushProperty(string tag, object value, string name = null)
        {
            var elem = new XElement(tag, new XElement("value", value));
            _scopes.Peek().Add(elem);
            if (name != null)
                elem.Add(new XAttribute("name", name));
        }

        public void PushSchemaElement(string tag, string name, string id = null)
        {
            _current = new XElement(tag, new XAttribute("name", name));
            _scopes.Peek().Add(_current);
            if (id != null)
                _current.Add(new XAttribute("id", id));
        }
    }
}
