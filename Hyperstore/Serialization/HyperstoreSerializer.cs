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
    ///  Serialization options.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum SerializationOptions
    {
        /// <summary>
        ///  Serialize using Hyperstore XML format
        /// </summary>
        Normal = 0,
        /// <summary>
        ///  Optimize schema persistence by creating a schemaid map.
        /// </summary>
        CompressSchema = 1,
        /// <summary>
        ///  Serialize as Json
        /// </summary>
        Json = 2
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Hyperstore serialization settings.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class SerializationSettings
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  JSon serializer used to serialize value object.
        /// </summary>
        /// <value>
        ///  A serializer or null to used the default serializer.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IJsonSerializer Serializer { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get or set options for controlling the serialization.
        /// </summary>
        /// <value>
        ///  The options.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SerializationOptions Options { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get or set a specific schema used to serialize elements.
        /// </summary>
        /// <value>
        ///  A specific schema or null to used the schema associated with elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchema Schema { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new serialization settings with the default CompressAll option.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public SerializationSettings()
        {
            Options = SerializationOptions.CompressSchema;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Hyperstore serializer. Allow to serialize a domain or a list of elements in XML or JSon
    ///  format. Serialize domain as a graph with entity and separate relationshsips. Use this
    ///  serialize to communicate with others hyperstore instance.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public partial class HyperstoreSerializer
    {
        private readonly SerializationOptions _options;
        private readonly ISchema _schema;
        private Dictionary<Identity, MonikerEntry> _monikers;
        private int _monikerSequence;
        private readonly IDomainModel _domain;
        private IJsonSerializer _serializer;
        private ISerializerWriter _writer;

        #region static

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
        /// <param name="settings">
        ///  (Optional) options for controlling the operation.
        /// </param>
        /// <param name="elements">
        ///  (Optional) the elements.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public static void Serialize(Stream stream, IDomainModel domain, SerializationSettings settings = null, IEnumerable<IModelElement> elements = null)
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="settings">
        ///  (Optional) options for controlling the operation.
        /// </param>
        /// <param name="elements">
        ///  (Optional) the elements.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string Serialize(IDomainModel domain, SerializationSettings settings = null, IEnumerable<IModelElement> elements = null)
        {
            string result = null;
            using (var writer = new MemoryStream())
            {
                Serialize(writer, domain, settings, elements);
                writer.Position = 0;
                using (var reader = new StreamReader(writer))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        #endregion

        private HyperstoreSerializer(IDomainModel domain, SerializationSettings settings)
        {
            Contract.Requires(domain, "domain");
            _domain = domain;
            _options = settings.Options;
            _schema = settings.Schema;
            _serializer = settings.Serializer;
            _writer = HasOption(SerializationOptions.Json) ? (ISerializerWriter)new JsonWriter(_options, domain) : new XmlWriter(_options, domain);
        }

        private string GetSchemaMoniker(IModelElement mel)
        {
            MonikerEntry moniker;
            if (HasOption(SerializationOptions.CompressSchema))
            {
                if (_monikers.TryGetValue(mel.SchemaInfo.Id, out moniker))
                    return moniker.Moniker;
            }

            var schemaInfo = GetSchemaInfo(mel, false);

            if (HasOption(SerializationOptions.CompressSchema))
            {
                MonikerEntry entry;
                _monikerSequence++;
                entry.Moniker = _monikerSequence.ToString();
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

        private string GetId(IModelElement element)
        {
            if (String.Compare(element.Id.DomainModelName, _domain.Name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return element.Id.Key;
            }

            return element.Id.ToString();
        }

        private bool HasOption(SerializationOptions option)
        {
            return (_options & option) == option;
        }

        private void Serialize(Stream stream, IEnumerable<IModelEntity> entities, IEnumerable<IModelRelationship> relationships)
        {
            ISession session = null;
            if (Session.Current != null)
            {
                Session.Current.SetMode(SessionMode.Serializing);
            }
            else
            {
                IHyperstore store = _domain.Store;
                SessionConfiguration sessionConfiguration = new SessionConfiguration();
                sessionConfiguration.Mode = SessionMode.Serializing;
                session = store.BeginSession(sessionConfiguration);
            }

            try
            {
                _monikers = new Dictionary<Identity, MonikerEntry>();
                SerializeEntities(entities);
                SerializeRelationships(relationships);
                
                _writer.Save(stream, _monikers.Values);
            }
            finally
            {
                if (session != null)
                {
                    session.AcceptChanges();
                    session.Dispose();
                }
                _monikers = null;
                _monikerSequence = 0;
            }
        }

        private void SerializeRelationships(IEnumerable<IModelRelationship> relationships)
        {
            _writer.NewScope("relationships");
            foreach (var relationship in relationships)
            {
                _writer.PushElement("relationship",
                            GetId(relationship), GetSchemaMoniker(relationship),
                            GetId(relationship.Start), GetSchemaMoniker(relationship.Start),
                            GetId(relationship.End), GetSchemaMoniker(relationship.End));
                SerializeProperties(relationship);
            }

            _writer.ReduceScope();
        }

        private void SerializeEntities(IEnumerable<IModelEntity> entities)
        {
            _writer.NewScope("entities");
            foreach (var entity in entities)
            {
                _writer.PushElement("entity", GetId(entity), GetSchemaMoniker(entity));
                SerializeProperties(entity);
            }

            _writer.ReduceScope();
        }

        private void SerializeProperties(IModelElement element)
        {
            var schemaInfo = GetSchemaInfo(element);
            foreach (var prop in schemaInfo.GetProperties(true))
            {
                var value = element.GetPropertyValue(prop);
                if (value.HasValue)
                {
                    _writer.PushProperty("property", prop.Name, Platform.PlatformServices.Current.ObjectSerializer.Serialize(prop.Serialize(value.Value)));
                }
            }
        }
    }
}
