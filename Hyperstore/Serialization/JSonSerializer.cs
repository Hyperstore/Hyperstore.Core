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
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  JSon serialization options
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum JSonSerializationOption
    {
        /// <summary>
        ///  Serialize schema
        /// </summary>
        SerializeSchema = 1,
        /// <summary>
        ///  Serialize IModelElement identity
        /// </summary>
        SerializeIdentity = 2,
        /// <summary>
        ///  Serialize embedded element by ref
        /// </summary>
        SerializeByReference = 6, // 4 + 2 Identity must be referenced
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
        Hyperstore = 63, // 32 + 16 + 8 + 1 + 2 + 4
        /// <summary>
        ///  Standard json format 
        /// </summary>
        Json = 256 + 1024,
        /// <summary>
        ///  Format json result
        /// </summary>
        Indent = 512,
        /// <summary>
        ///  Serialize only elements defined in parameter
        /// </summary>
        SerializeGraphObject = 1024
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  JSon serialization settings.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class JSonSerializationSettings
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  JSon serializer used to serialize value object.
        /// </summary>
        /// <value>
        ///  A serializer or null to used the default serializer
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IJsonSerializer Serializer { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serialization options
        /// </summary>
        /// <value>
        ///  The options.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public JSonSerializationOption Options { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Schema to used to serialize domain elements. 
        /// </summary>
        /// <value>
        ///  A schema to replace the schema associated with each elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchema Schema { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new serialization settings with default Json option
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public JSonSerializationSettings()
        {
            Options = JSonSerializationOption.Json;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A Json domain model serializer.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class JSonSerializer
    {
        private readonly TextWriter _writer;
        private IJsonSerializer _serializer;
        private Dictionary<Identity, int> _schemaElements;
        private HashSet<IModelElement> _toSerialize;
        private HashSet<Identity> _serialized;
        private readonly JSonSerializationOption _options;
        private int _depth;
        private Queue<IModelElement> _elements;
        private readonly ISchema _schema;

        #region static

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serialize a domain with the sepecified options
        /// </summary>
        /// <param name="domain">
        ///  Domain to serialize
        /// </param>
        /// <param name="option">
        ///  (Optional) the option.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string Serialize(IDomainModel domain, JSonSerializationOption option = JSonSerializationOption.Json)
        {
            return Serialize(domain, new JSonSerializationSettings { Options = option });
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="stream">
        ///  The stream.
        /// </param>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="option">
        ///  (Optional) the option.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public static void Serialize(Stream stream, IDomainModel domain, JSonSerializationOption option = JSonSerializationOption.Json)
        {
            using (var sw = new StreamWriter(stream))
            {
                var ser = new JSonSerializer(sw, new JSonSerializationSettings { Options = option });
                ser.Serialize( domain.GetElements());
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        /// <param name="option">
        ///  (Optional) the option.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string Serialize(IEnumerable<IModelElement> elements, JSonSerializationOption option = JSonSerializationOption.Json)
        {
            return Serialize(elements, new JSonSerializationSettings { Options = option });
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="mel">
        ///  The mel.
        /// </param>
        /// <param name="option">
        ///  (Optional) the option.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string Serialize(IModelElement mel, JSonSerializationOption option = JSonSerializationOption.Json)
        {
            return Serialize(mel, new JSonSerializationSettings { Options = option });
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="settings">
        ///  Options for controlling the operation.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        /// <param name="settings">
        ///  Options for controlling the operation.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string Serialize(IEnumerable<IModelElement> elements, JSonSerializationSettings settings)
        {
            Contract.Requires(elements, "elements");
            Contract.Requires(settings, "settings");

            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            var ser = new JSonSerializer(sw, settings);
            ser.Serialize(elements);
            return sw.ToString();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="mel">
        ///  The mel.
        /// </param>
        /// <param name="settings">
        ///  Options for controlling the operation.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string Serialize(IModelElement mel, JSonSerializationSettings settings)
        {
            Contract.Requires(mel, "mel");

            var list = new List<IModelElement>();
            list.Add(mel);

            return Serialize(list, settings);
        }
        #endregion

        private JSonSerializer(TextWriter writer, JSonSerializationSettings settings = null)
        {
            Contract.Requires(writer, "writer");

            _writer = writer;
            _options = JSonSerializationOption.Json;
            if (settings != null)
            {
                _options = settings.Options;
                _serializer = settings.Serializer;
                _schema = settings.Schema;
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
            if (_elements.Count != 1)
            {
                Write('[', indent);
                _depth++;
                array = true;
            }

            bool first = true;
            while (_elements.Count > 0)
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

        private void Write(string text, bool indent = true)
        {
             WriteIndent(indent);
             _writer.Write(text);
        }

        private void Write(char ch, bool indent = false)
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

        private ISchemaElement GetSchemaInfo(IModelElement mel)
        {
            return _schema == null ? mel.SchemaInfo : _schema.GetSchemaElement(mel.SchemaInfo.Id);
        }

        private void SerializeElement(IModelElement element, bool insertComma)
        {
            if (element == null || !_serialized.Add(element.Id))
                return;

            if (element is IModelRelationship && !HasOption(JSonSerializationOption.SerializeRelationship))
                return;

            insertComma = WriteStartElement(element, insertComma);

            var schemaInfo = GetSchemaInfo(element);

            var schema = schemaInfo.Schema;
            foreach (var relationship in schema.GetRelationships(start: schemaInfo))
            {
                var relationshipSchema = relationship as ISchemaRelationship;
                if (relationshipSchema != null && relationshipSchema.StartPropertyName != null)
                {
                    insertComma = SerializeReference(element, relationshipSchema, true, insertComma);
                }
            }

            // Opposites
            foreach (var relationship in schema.GetRelationships(end: schemaInfo))
            {
                var relationshipSchema = relationship as ISchemaRelationship;
                if (relationshipSchema != null && relationshipSchema.EndPropertyName != null)//&& relationshipSchema.StartPropertyName == null)                 // Only if the relationship has not been serialized previously (StartPropertyName != null)
                {
                    insertComma = SerializeReference(element, relationshipSchema, false, insertComma);
                }
            }

            foreach (var prop in schemaInfo.GetProperties(true))
            {
                var value = element.GetPropertyValue(prop);
                if (value.HasValue)
                {
                    WriteKeyValue(prop.Name, prop.Serialize(value.Value, _serializer), insertComma);
                    insertComma = true;
                }
            }

            _depth--;
            WriteEndElement();
        }

        private bool SerializeReference(IModelElement element, ISchemaRelationship relationshipSchema, bool outDirection, bool insertComma)
        {
            bool many = (outDirection && (relationshipSchema.Cardinality & Cardinality.OneToMany) == Cardinality.OneToMany) || (!outDirection && (relationshipSchema.Cardinality & Cardinality.ManyToOne) == Cardinality.ManyToOne);
            bool first = !insertComma || !many;

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

                if (HasOption(JSonSerializationOption.Json))
                {
                    if (IsReference(terminal))
                    {
                        //Write('"', true);
                        //Write(outDirection ? relationshipSchema.EndPropertyName : relationshipSchema.StartPropertyName, false);
                        //Write('"');
                        //Write(':');
                        Write('{', true);
                        _depth++;
                        WriteJsonId(terminal, false, true);
                        _depth--;
                        Write('}', true);
                        continue;
                    }
                }

                if (!HasOption(JSonSerializationOption.Json) && (HasOption(JSonSerializationOption.SerializeByReference) || _serialized.Contains(terminal.Id)))
                {
                    Write('{', true);
                    _depth++;
                    if (!HasOption(JSonSerializationOption.SerializeIdentity))
                        throw new JsonSerializationException("Circular dependency detected. You must set the JSonSerializationOption.SerializeIdentity to serialize this object graph.");

                    WriteKeyString("_eid", terminal.Id.ToString(), false);

                    if (HasOption(JSonSerializationOption.SerializeRelationship))
                    {
                        WriteKeyString("_rid", rel.Id.ToString());
                    }
                    if (HasOption(JSonSerializationOption.SerializeSchemaIdentity))
                    {
                        WriteKeyValue("_eshid", GetSchemaIndex(GetSchemaInfo(terminal).Id));
                        WriteKeyValue("_rshid", GetSchemaIndex(GetSchemaInfo(rel).Id));
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

        private Dictionary<Identity, string> _identityMaps;
        private int _sequence;

        private bool WriteStartElement(IModelElement mel, bool insertComma)
        {
            if (insertComma)
                Write(',');

            Write('{', true);
            _depth++;
            insertComma = false;
            if (HasOption(JSonSerializationOption.Json))
            {
                WriteJsonId(mel, insertComma);
                insertComma = true;
            }
            else
            {
                if (HasOption(JSonSerializationOption.SerializeIdentity))
                {
                    WriteKeyString("_id", mel.Id.ToString(), insertComma);
                    insertComma = true;
                }

                if (HasOption(JSonSerializationOption.SerializeSchemaIdentity))
                {
                    WriteKeyValue("_shid", GetSchemaIndex(GetSchemaInfo(mel).Id), insertComma);
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
                        WriteKeyValue("_sshid", GetSchemaIndex(GetSchemaInfo(rel.Start).Id));
                        WriteKeyValue("_eshid", GetSchemaIndex(GetSchemaInfo(rel.End).Id));
                    }
                }
            }

            return insertComma;
        }

        private bool IsReference(IModelElement mel)
        {
            string id;
            return _identityMaps != null && _identityMaps.TryGetValue(mel.Id, out id);
        }

        private void WriteJsonId(IModelElement mel, bool insertComma, bool isRef = false)
        {
            if (_identityMaps == null)
            {
                _identityMaps = new Dictionary<Identity, string>();
                _sequence = 0;
            }

            string id;
            if (!_identityMaps.TryGetValue(mel.Id, out id))
            {
                _sequence++;
                id = _sequence.ToString();
                _identityMaps.Add(mel.Id, id);
            }

            WriteKeyString(isRef ? "$ref" : "$id", id, insertComma);
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
