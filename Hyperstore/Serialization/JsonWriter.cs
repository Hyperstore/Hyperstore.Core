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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Serialization
{
    static class StringBuilderEx
    {
        public static void AppendString(this StringBuilder sb, string value)
        {
            sb.Append("\"");
            sb.Append(value);
            sb.Append("\"");
        }
        public static void AppendKey(this StringBuilder sb, string name, bool prefixWithColon = false)
        {
            if (prefixWithColon)
                sb.Append(",");
            AppendString(sb, name);
            sb.Append(":");
        }
    }

    class JsonWriter : ISerializerWriter
    {
        private Stack<List<string>> _scopes = new Stack<List<string>>();

        public JsonWriter()
        {
            NewScope();
        }

        public void NewScope()
        {
            _scopes.Push(new List<string>());
        }


        public void PushElement(string name, string id, string schemaId = null)
        {
            var list = _scopes.Pop();
            var current = new StringBuilder("{");

            current.AppendKey("id");
            current.AppendString(id);
            if (schemaId != null)
            {
                current.AppendKey("schema", true);
                current.AppendString(schemaId);
            }
            if (list.Count > 0)
            {
                current.AppendKey("properties", true);
                current.Append("[");
                current.Append(String.Join(",", list));
                current.Append("]");
            }
            current.Append("}");
            _scopes.Peek().Add(current.ToString());
        }

        public void PushElement(string name, string id, string startId, string startSchemaId, string endId, string endSchemaId, string schemaId = null)
        {
            var list = _scopes.Pop();
            var current = new StringBuilder("{");

            current.AppendKey("id");
            current.AppendString(id);
            current.AppendKey("startId", true);
            current.AppendString(startId);
            current.AppendKey("startSchemaId", true);
            current.AppendString(startSchemaId);
            current.AppendKey("endId", true);
            current.AppendString(endId);
            current.AppendKey("endSchemaId", true);
            current.AppendString(endSchemaId);
            if (schemaId != null)
            {
                current.AppendKey("schema", true);
                current.AppendString(schemaId);
            }
            if (list.Count > 0)
            {
                current.AppendKey("properties", true);
                current.Append("[");
                current.Append(String.Join(",", list));
                current.Append("]");
            }
            current.Append("}");
            _scopes.Peek().Add(current.ToString());
        }

        public void PushProperty(string tag, object value, string name = null)
        {
            _scopes.Peek().Add(String.Format("{{ \"name\":\"{0}\", \"value\":{1} }}", name ?? tag, value));
        }

        public void PushSchemaElement(string tag, string name, string id = null)
        {
            var current = new StringBuilder("{");

            current.AppendKey("id");
            current.AppendString(id);
            current.AppendKey("name", true);
            current.AppendString(name);
            if (id != null)
            {
                current.AppendKey("schema", true);
                current.AppendString(id);
            }
            current.Append("}");
            _scopes.Peek().Add(current.ToString());
        }

        public void ReduceScope(string tag, string name = null, bool unshift = false)
        {
            var list = _scopes.Pop();
            if (list.Count > 0)
            {
                var current = new StringBuilder();
                if (name != null)
                {
                    current.Append("{");
                    current.AppendFormat(String.Format("\"name\":\"{0}\", \"elements\":", name));
                }
                else
                    current.AppendKey(tag);

                current.Append("[");
                current.Append(String.Join(", ", list));
                current.Append("]");
                if (name != null)
                    current.Append("}");

                if (!unshift)
                    _scopes.Peek().Add(current.ToString());
                else
                    _scopes.Peek().Insert(0, current.ToString());

            }
        }

        public void SaveTo(System.IO.Stream stream, IDomainModel domain)
        {
            var writer = new System.IO.StreamWriter(stream);

            var list = _scopes.Pop();
            writer.Write("{");
            writer.Write("\"name\":\"{0}\",", domain.Name);
            if (domain.ExtensionName != null)
                writer.Write("\"extension\":\"{0}\",", domain.ExtensionName);
            if (list.Count > 0)
                writer.Write(String.Join(",", list));
            writer.Write("}");
            writer.Flush();
        }
    }
}
