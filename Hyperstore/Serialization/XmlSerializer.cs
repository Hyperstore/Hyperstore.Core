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
    ///  Bitfield of flags for specifying JSonSerializationOption.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum SerializationOptions
    {
        /// <summary>
        ///  Serialize elements
        /// </summary>
        Normal = 0,
        /// <summary>
        ///  Compress data
        /// </summary>
        CompressSchemaId = 1,

        /// <summary>
        ///  Specifies the compress id= 2 option.
        /// </summary>
        CompressId = 2,

        /// <summary>
        ///  Specifies the compress all= 3 option.
        /// </summary>
        CompressAll = 3,

        /// <summary>
        ///  reserved
        /// </summary>
        Fluent = 4,

        /// <summary>
        ///  Specifies the json option.
        /// </summary>
        Json = 8
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A son serialization settings.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class SerializationSettings
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the serializer.
        /// </summary>
        /// <value>
        ///  The serializer.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IJsonSerializer Serializer { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets options for controlling the operation.
        /// </summary>
        /// <value>
        ///  The options.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SerializationOptions Options { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets a specific schema used to serialize elements.
        /// </summary>
        /// <value>
        ///  The schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchema Schema { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public SerializationSettings()
        {
            Options = SerializationOptions.CompressAll;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A domain model serializer.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public partial class HyperstoreSerializer
    {
        struct MonikerEntry
        {
            public string Moniker;
            public ISchemaElement Schema;
            public string SchemaName;
        }

        private readonly SerializationOptions _options;
        private readonly ISchema _schema;
        private Dictionary<Identity, MonikerEntry> _monikers;
        private int _monikerSequence;
        private readonly IDomainModel _domain;
        private IJsonSerializer _serializer;
        private ISerializerWriter _writer;

        #region static

        public static void Serialize(Stream stream, IDomainModel domain, SerializationSettings settings=null, IEnumerable<IModelElement> elements=null)
        {
            Contract.Requires(stream, "stream");
            Contract.Requires(domain, "domain");

            if (settings == null)
                settings = new SerializationSettings();
            var ser = new HyperstoreSerializer(domain, settings);
            ser.Serialize(stream, 
                elements != null ? elements.OfType<IModelEntity>() : domain.GetEntities(),
                elements != null ? elements.OfType<IModelRelationship>() : domain.GetRelationships());
        }

        public static string Serialize(IDomainModel domain, SerializationSettings settings=null, IEnumerable<IModelElement> elements = null)
        {
            string result = null;
            using (var writer = new MemoryStream())
            {
                Serialize(writer, domain, settings, elements);
                writer.Position = 0;
                using (var reader = new StreamReader(writer ))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        #endregion

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domain">
        /// The domain to serialize
        /// </param>
        /// <param name="settings">
        ///  Options for controlling the operation.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        private HyperstoreSerializer(IDomainModel domain, SerializationSettings settings)
        {
            Contract.Requires(domain, "domain");
            _domain = domain;
            _options = settings.Options;
            _schema = settings.Schema;
            _serializer = settings.Serializer;
            _writer = HasOption(SerializationOptions.Json) ? (ISerializerWriter)new JsonWriter() : new XmlWriter();
        }

        private string GetSchemaMoniker(IModelElement mel)
        {
            MonikerEntry moniker;
            if (HasOption(SerializationOptions.CompressSchemaId) || HasOption(SerializationOptions.Fluent))
            {
                if (_monikers.TryGetValue(mel.SchemaInfo.Id, out moniker))
                    return moniker.Moniker;
            }

            var schemaInfo = GetSchemaInfo(mel, false);

            if (HasOption(SerializationOptions.CompressSchemaId) || HasOption(SerializationOptions.Fluent))
            {
                MonikerEntry entry;
                if (HasOption(SerializationOptions.Fluent))
                {
                    entry.Moniker = schemaInfo.Id.Key;
                }
                else
                {
                    _monikerSequence++;
                    entry.Moniker = _monikerSequence.ToString();
                }
                entry.Schema = schemaInfo;
                entry.SchemaName = schemaInfo.Id.DomainModelName;
                _monikers[mel.SchemaInfo.Id] = entry;
                return entry.Moniker;
            }

            return schemaInfo.Id.ToString();
        }

        private ISchemaElement GetSchemaInfo(IModelElement mel, bool findInMoniker = false)
        {
            if (_monikers != null && findInMoniker)
            {
                MonikerEntry moniker;
                if (_monikers.TryGetValue(mel.SchemaInfo.Id, out moniker))
                    return moniker.Schema;
            }

            return _schema == null ? mel.SchemaInfo : _schema.GetSchemaElement(mel.SchemaInfo.Id);
        }

        private bool HasOption(SerializationOptions option)
        {
            return (_options & option) == option;
        }

        private void Serialize(Stream stream, IEnumerable<IModelEntity> entities, IEnumerable<IModelRelationship> relationships)
        {
            try
            {
                _monikers = new Dictionary<Identity, MonikerEntry>();
                SerializeEntities(entities);
                SerializeRelationships(relationships);

                if (HasOption(SerializationOptions.CompressSchemaId) || HasOption(SerializationOptions.Fluent))
                {
                    _writer.NewScope();

                    foreach (var entries in _monikers.Values.GroupBy(s => s.SchemaName))
                    {
                        _writer.NewScope();

                        foreach (var entry in entries)
                        {
                            _writer.PushSchemaElement("add", entry.Schema.Id.Key, HasOption(SerializationOptions.CompressSchemaId)
                            ? entry.Moniker : null);
                        }
                        _writer.ReduceScope("schema", entries.Key);
                    }
                    _writer.ReduceScope("schemas", unshift: true);
                }

                _writer.SaveTo(stream, _domain);
            }
            finally
            {
                _monikers = null;
                _monikerSequence = 0;
            }
        }

        private void SerializeRelationships(IEnumerable<IModelRelationship> relationships)
        {
            _writer.NewScope();
            foreach (var relationship in relationships)
            {
                SerializeProperties(relationship);
                if (HasOption(SerializationOptions.Fluent))
                {
                    _writer.PushElement(GetSchemaMoniker(relationship), GetId(relationship), GetId(relationship.Start), GetSchemaMoniker(relationship.Start),
                        GetId(relationship.End), GetSchemaMoniker(relationship.End));
                }
                else
                {
                    _writer.PushElement("relationship", GetId(relationship), GetId(relationship.Start), GetSchemaMoniker(relationship.Start),
                        GetId(relationship.End), GetSchemaMoniker(relationship.End), GetSchemaMoniker(relationship));
                }
            }

            _writer.ReduceScope("relationships");
        }

        private string GetId(IModelElement element)
        {
            if (HasOption(SerializationOptions.CompressId) && String.Compare(element.Id.DomainModelName, _domain.Name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return element.Id.Key;
            }

            return element.Id.ToString();
        }

        private void SerializeEntities(IEnumerable<IModelEntity> entities)
        {
            _writer.NewScope();
            foreach (var entity in entities)
            {
                SerializeProperties(entity);

                if (HasOption(SerializationOptions.Fluent))
                {
                    _writer.PushElement(GetSchemaMoniker(entity), GetId(entity));
                }
                else
                {
                    _writer.PushElement("entity", GetId(entity), GetSchemaMoniker(entity));
                }
            }

            _writer.ReduceScope("entities");
        }

        private void SerializeProperties(IModelElement element)
        {
            _writer.NewScope();
            var schemaInfo = GetSchemaInfo(element);

            foreach (var prop in schemaInfo.GetProperties(true))
            {
                var value = element.GetPropertyValue(prop);
                if (value.HasValue)
                {
                    if (HasOption(SerializationOptions.Fluent))
                    {
                        _writer.PushProperty(prop.Name, prop.Serialize(value.Value, _serializer));
                    }
                    else
                    {
                        _writer.PushProperty("property", prop.Serialize(value.Value, _serializer), prop.Name);
                    }
                }
            }
        }
    }
}
