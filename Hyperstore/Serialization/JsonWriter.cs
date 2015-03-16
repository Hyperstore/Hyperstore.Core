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

namespace Hyperstore.Modeling.Serialization
{
    static class StringBuilderEx
    {

    }

    class JsonWriter : ISerializerWriter
    {
        private StringWriter _stream = new StringWriter();
        private SerializationOptions _options;
        private bool _propertiesOpen;
        private bool _firstElement;
        private bool _firstScope;
        private IDomainModel _domain;

        public JsonWriter(SerializationOptions options, IDomainModel domain)
        {
            this._options = options;
            _stream = new StringWriter();
            _firstScope = true;
            _domain = domain;
        }

        private bool HasOption(SerializationOptions option)
        {
            return (_options & option) == option;
        }

        public void NewScope(string tag)
        {
            WriteKey(tag, !_firstScope);
            Write("[");
            _firstElement = true;
            _firstScope = false;
            _propertiesOpen = false;
        }

        public void ReduceScope()
        {
            if (_propertiesOpen)
                Write("]");
            Write("}");
            Write("]");
        }

        public void PushDeletedElement(string name, string id)
        {
            if (_propertiesOpen)
                Write("]");
            if (!_firstElement)
                Write("},");
            Write("{");
            WriteKey("id");
            WriteString(id);
            WriteKey("deleted", true);
            _stream.Write("true");

            _propertiesOpen = false;
            _firstElement = false;
        }

        public void PushElement(string name, string id, string schemaId, string startId = null, string startSchemaId = null, string endId = null, string endSchemaId = null)
        {
            if (_propertiesOpen)
                Write("]");
            if (!_firstElement)
                Write("},");
            Write("{");
            WriteKey("id");
            WriteString(id);
            WriteKey("schema", true);
            WriteString(schemaId);

            if (startId != null)
            {
                WriteKey("startId", true);
                WriteString(startId);
                WriteKey("startSchemaId", true);
                WriteString(startSchemaId);
                WriteKey("endId", true);
                WriteString(endId);
                WriteKey("endSchemaId", true);
                WriteString(endSchemaId);
            }
            _propertiesOpen = false;
            _firstElement = false;
        }

        public void PushProperty(string tag, string name, object value)
        {
            if (!_propertiesOpen)
            {
                WriteKey("properties", true);
                Write("[");
                _propertiesOpen = true;
            }
            else
            {
                Write(",");
            }
            _stream.Write("{{ \"name\":\"{0}\", \"value\":{1} }}", name, value);
        }

        public void SaveSchema(System.IO.StreamWriter writer, IEnumerable<MonikerEntry> monikers)
        {
            if (HasOption(SerializationOptions.CompressSchema) && monikers.Any())
            {
                writer.Write("\"schemas\" : [");
                bool first = true;
                foreach (var schemas in monikers.GroupBy(s => s.SchemaName))
                {
                    if (!first)
                        writer.Write(",");
                    first = false;
                    writer.Write("{{ \"name\" : \"{0}\", \"elements\" : [", schemas.Key);
                    bool first2 = true;
                    foreach (var moniker in schemas)
                    {
                        if (!first2)
                            writer.Write(",");
                        first2 = false;
                        writer.Write("{{ \"id\" : \"{0}\", \"name\":\"{1}\"}}", moniker.Moniker, moniker.Schema.Id.Key);
                    }
                    writer.Write("] }");
                }

                writer.Write("], ");
            }
        }

        public void Save(Stream stream, IEnumerable<MonikerEntry> monikers)
        {
            var writer = new System.IO.StreamWriter(stream);

            writer.Write("{");
            writer.Write("\"name\":\"{0}\",", _domain.Name);
            if (_domain.ExtensionName != null)
                writer.Write("\"extension\":\"{0}\",", _domain.ExtensionName);
            SaveSchema(writer, monikers);
            _stream.Flush();
            writer.Write(_stream.ToString());
            writer.Write("}");
            writer.Flush();
        }

        void Write(string txt)
        {
            _stream.Write(txt);
        }

        void WriteString(string value)
        {
            _stream.Write("\"");
            _stream.Write(value);
            _stream.Write("\"");
        }

        void WriteKey(string name, bool prefixWithColon = false)
        {
            if (prefixWithColon)
                _stream.Write(",");
            WriteString(name);
            _stream.Write(":");
        }
    }
}
