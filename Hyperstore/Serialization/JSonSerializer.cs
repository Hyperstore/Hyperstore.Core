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

using Hyperstore.Modeling.Platform;
using Hyperstore.Modeling.Traversal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Serialization
{
    [Flags]
    public enum JSonSerializationOption
    {
        /// <summary>
        ///  Serialize schema
        /// </summary>
        SerializeSchema=1,
        /// <summary>
        ///  Serialize IModelElement identity
        /// </summary>
        SerializeIdentity=2,
        /// <summary>
        ///  Serialize embedded element by ref
        /// </summary>
        SerializeByReference=6, // 4 + 2 Identity must be referenced
        /// <summary>
        ///  Include relationship
        /// </summary>
        SerializeRelationship = 8,
        /// <summary>
        ///  Serialize schema identity
        /// </summary>
        SerializeSchemaIdentity = 16,
        /// <summary>
        ///  Serialize all hyperstore informations (schema and id)
        /// </summary>
        Hyperstore=63, // 32 + 16 + 8 + 1 + 2 + 4
        /// <summary>
        ///  Standard json format 
        /// </summary>
        Json = 256 + 1024,
        /// <summary>
        ///  Format json result
        /// </summary>
        Indent=512, 
        /// <summary>
        ///  Serialize only elements defined in parameter
        /// </summary>
        SerializeGraphObject=1024
    }

    public class JSonSerializationSettings
    {
        public IJsonSerializer Serializer { get; set; }
        public JSonSerializationOption Options { get; set; }

        public JSonSerializationSettings()
        {
            Options = JSonSerializationOption.Json;
        }
    }

    public class JSonDomainModelSerializer
    {
        private readonly TextWriter _writer;
        private IJsonSerializer _serializer;
        private Dictionary<Identity, int> _schemaElements;
        private HashSet<IModelElement> _toSerialize;
        private HashSet<Identity> _serialized;
        private readonly JSonSerializationOption _options;
        private int _depth;
        private Queue<IModelElement> _elements;

        #region static
        public static string Serialize(IDomainModel domain, JSonSerializationOption option = JSonSerializationOption.Json)
        {
            return Serialize(domain, new JSonSerializationSettings { Options = option });
        }

        public static string Serialize(IEnumerable<IModelElement> elements, JSonSerializationOption option = JSonSerializationOption.Json)
        {
            return Serialize(elements, new JSonSerializationSettings { Options = option });
        }

        public static string Serialize(IModelElement mel, JSonSerializationOption option = JSonSerializationOption.Json, ITraversalQuery query = null)
        {
            return Serialize(mel, new JSonSerializationSettings { Options = option }, query);
        }

        public static string Serialize(IDomainModel domain, JSonSerializationSettings settings)
        {
            Contract.Requires(domain, "domain");

            // Force hyperstore format
            if (settings == null)
                settings = new JSonSerializationSettings();
            settings.Options |= JSonSerializationOption.Hyperstore;
            settings.Options &= ~JSonSerializationOption.SerializeGraphObject;
            return Serialize(domain.GetElements(), settings);
        }

        public static string Serialize(IEnumerable<IModelElement> elements, JSonSerializationSettings settings)
        {
            Contract.Requires(elements, "elements");

            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            var ser = new JSonDomainModelSerializer(sw, settings);
            ser.Serialize(elements);
            return sw.ToString();
        }

        public static string Serialize(IModelElement mel, JSonSerializationSettings settings, ITraversalQuery query = null)
        {
            Contract.Requires(mel, "mel");

            
            var list = new List<IModelElement>();
            list.Add(mel);
            
            if (query != null)
            {
                if (settings == null)
                    settings = new JSonSerializationSettings();
                settings.Options &= ~JSonSerializationOption.SerializeGraphObject;

                foreach (var path in query.GetPaths(mel))
                {
                    list.Add(path.EndElement);
                    list.Add(path.LastTraversedRelationship);
                }
            }

            return Serialize(list, settings);
        }
        #endregion

        private JSonDomainModelSerializer(TextWriter writer, JSonSerializationSettings settings = null)
        {
            Contract.Requires(writer, "writer");

            _writer = writer;
            _options = JSonSerializationOption.Json;
            if (settings != null)
            {
                _options = settings.Options;
                _serializer = settings.Serializer;
            }

            if (((int)_options & 32) != 0) // Hyperstore guard
                _options |= JSonSerializationOption.Hyperstore;
            else
            {
                if (((int)_options & 256) == 0)
                    throw new ArgumentException("Either Json or Hyperstore format must be set in the option parameter");
            }
        }

        private bool HasOption(JSonSerializationOption option)
        {
            return (_options & option) == option;
        }


        private void Serialize(IEnumerable<IModelElement> elements)
        {
            _serialized = new HashSet<Identity>();
            _schemaElements = new Dictionary<Identity, int>();
            _elements = new Queue<IModelElement>(elements);

            if (!HasOption(JSonSerializationOption.SerializeGraphObject))
            {
                _toSerialize = new HashSet<IModelElement>(elements, new ModelElementComparer());
            }

            SerializeElements();
        }

        private void SerializeElements()
        {
            bool indent = false;

            if (HasOption(JSonSerializationOption.SerializeSchema))
            {
                Write('{', true);
                _depth++;
                Write("\"elements\":");
                indent = true;
            }

            bool array = false;
            if (_elements.Count != 1 || HasOption(JSonSerializationOption.SerializeGraphObject))
            {
                Write('[', indent);
                _depth++;
                array = true;
            }

            bool first = true;
            while(_elements.Count > 0)
            {
                var mel = _elements.Dequeue();
                SerializeElement(mel, !first);
                first = false;
            }

            if (array)
            {
                _depth--;
                Write("]", true);
            }

            if (HasOption(JSonSerializationOption.SerializeSchema))
            {
                Write(',', true);
                _depth++;
                Write("\"schemas\":");
                Write('[', true);
                _depth++;
                first = true;
                foreach (var kv in _schemaElements.OrderBy(d => d.Value))
                {
                    if (!first)
                        Write(',');
                    first = false;
                    Write('"');
                    Write(kv.Key.ToString());
                    Write('"');
                }
                _depth--;
                Write("]");
                _depth--;
                Write('}', true);
            }
        }

        private void Write(string text, bool indent=true)
        {
            WriteIndent(indent);
            _writer.Write(text);
        }

        private void Write(char ch, bool indent=false)
        {
            WriteIndent(indent);
            _writer.Write(ch);
        }

        private void WriteIndent(bool indent)
        {
            if (HasOption(JSonSerializationOption.Indent))
            {
                _writer.WriteLine();
                for (int i = 0; i < _depth * 2; i++)
                {
                    _writer.Write(' ');
                }
            }
        }


        private void SerializeElement(IModelElement element, bool insertComma)
        {
            if (element == null || !_serialized.Add(element.Id))
                return;

            if (element is IModelRelationship && !HasOption(JSonSerializationOption.SerializeRelationship))
                return;

            insertComma = WriteStartElement(element, insertComma);
            foreach (var prop in element.SchemaInfo.GetProperties(true))
            {
                var value = element.GetPropertyValue(prop);
                if (value.HasValue)
                {
                    WriteKeyValue(prop.Name, prop.Serialize(value.Value, _serializer), insertComma);
                    insertComma = true;
                }
            }

            var schema = element.SchemaInfo.Schema;
            foreach (var relationship in schema.GetRelationships(start:element.SchemaInfo))
            {
                var relationshipSchema = relationship as ISchemaRelationship;
                if (relationshipSchema != null && relationshipSchema.StartPropertyName != null)
                {
                    insertComma = SerializeReference(element, relationshipSchema, true, insertComma);
                }
            }

            foreach (var relationship in schema.GetRelationships(end: element.SchemaInfo))
            {
                var relationshipSchema = relationship as ISchemaRelationship;
                if (relationshipSchema != null && relationshipSchema.EndPropertyName != null)
                {
                    insertComma = SerializeReference(element, relationshipSchema, false, insertComma);
                }
            }

            _depth--;
            WriteEndElement();
        }

        private bool SerializeReference(IModelElement element, ISchemaRelationship relationshipSchema, bool outDirection, bool insertComma)
        {
            bool many = relationshipSchema.Cardinality == Cardinality.ManyToMany || relationshipSchema.Cardinality == Cardinality.OneToMany;
            bool first = !insertComma;

            if (insertComma)
                Write(',');
            insertComma = true;

            Write('"', true);
            Write(outDirection ? relationshipSchema.StartPropertyName : relationshipSchema.EndPropertyName, false);
            Write('"');
            Write(':');

            if (many)
            {
                Write('[', true);
                _depth++;
                first = true;
            }

            var query = outDirection ? element.GetRelationships(relationshipSchema) : element.DomainModel.GetRelationships(relationshipSchema, end: element);

            foreach (var rel in query)
            {
                if (!ShouldSerialize(rel))
                    continue;

                if (!first)
                    Write(',');
                first = false;

                var terminal = outDirection ? rel.End : rel.Start;
                if (terminal == null)
                    continue;

                if (HasOption(JSonSerializationOption.SerializeByReference) || _serialized.Contains(terminal.Id))
                {
                    Write('{', true);
                    _depth++;
                    if (!HasOption(JSonSerializationOption.SerializeIdentity))
                        throw new Exception("Circular dependency detected. You must set the JSonSerializationOption.SerializeIdentity to serialize this object grpah.");

                    WriteKeyString("_eid", terminal.Id.ToString(), false);

                    if (HasOption(JSonSerializationOption.SerializeRelationship))
                    {
                        WriteKeyString("_rid", rel.Id.ToString());
                    }
                    if (HasOption(JSonSerializationOption.SerializeSchemaIdentity))
                    {
                        WriteKeyValue("_eshid", GetSchemaIndex(terminal.SchemaInfo.Id));
                        WriteKeyValue("_rshid", GetSchemaIndex(rel.SchemaInfo.Id));
                    }
                    _depth--;
                    Write('}', true);
                }
                else
                {
                    SerializeElement(terminal, false);
                }
            }
            if (many)
            {
                _depth--;
                Write(']', true);
            }
            return insertComma;
        }

        private bool ShouldSerialize(IModelRelationship rel)
        {
            if (HasOption(JSonSerializationOption.SerializeGraphObject))
            {
                _elements.Enqueue(rel.End);
                if (HasOption(JSonSerializationOption.SerializeRelationship))
                    _elements.Enqueue(rel);
                return true;
            }

            return _toSerialize.Contains(rel);
        }

        private bool WriteStartElement(IModelElement mel, bool insertComma)
        {
            if (insertComma)
                Write(',');

            Write('{', true);
            _depth++;
            insertComma = false;
            if (HasOption(JSonSerializationOption.SerializeIdentity))
            {
                WriteKeyString("_id", mel.Id.ToString(), insertComma);
                insertComma = true;
            }

            if (HasOption(JSonSerializationOption.SerializeSchemaIdentity))
            {
                WriteKeyValue("_shid", GetSchemaIndex(mel.SchemaInfo.Id), insertComma);
                insertComma = true;
            }

            if (mel is IModelRelationship)
            {
                var rel = mel as IModelRelationship;
                if (HasOption(JSonSerializationOption.SerializeIdentity))
                {
                    WriteKeyString("_sid", rel.Start.Id.ToString());
                    WriteKeyString("_eid", rel.End.Id.ToString());
                }

                if (HasOption(JSonSerializationOption.SerializeSchemaIdentity))
                {
                    WriteKeyValue("_sshid", GetSchemaIndex(rel.Start.SchemaInfo.Id));
                    WriteKeyValue("_eshid", GetSchemaIndex(rel.End.SchemaInfo.Id));
                }
            }
            return insertComma;
        }

        private string GetSchemaIndex(Identity identity)
        {
            if (HasOption(JSonSerializationOption.SerializeSchema))
            {
                int value;
                if (!_schemaElements.TryGetValue(identity, out value))
                {
                    value = _schemaElements.Count;
                    _schemaElements.Add(identity, value);
                }
                return value.ToString();
            }

            return String.Format("\"{0}\"", identity);
        }

        private void WriteKeyValue(string key, string value, bool insertComma = true)
        {
            if (value == null)
                return;

            if (insertComma)
                Write(',');
            Write('"', true);
            Write(key, false);
            Write('"');
            Write(':');
            Write(value, false);
        }

        private void WriteKeyString(string key, string value, bool insertComma = true)
        {
            if (value == null)
                return;

            if (insertComma)
                Write(',');
            Write('"', true);
            Write(key, false);
            Write('"');
            Write(':');
            Write('"');
            Write(value, false);
            Write('"');
        }

        private void WriteEndElement()
        {
            Write('}', true);
        }

    }
}
