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
using System.Xml;
using System.Xml.Linq;

namespace Hyperstore.Modeling.Serialization
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Bitfield of flags for specifying JSonSerializationOption.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum XmlSerializationOption
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
        Fluent = 4
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A son serialization settings.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class XmlSerializationSettings
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
        public XmlSerializationOption Options { get; set; }

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
        public XmlSerializationSettings()
        {
            Options = XmlSerializationOption.Normal;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A son domain model serializer.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public partial class XmlSerializer
    {
        struct MonikerEntry
        {
            public string Moniker;
            public ISchemaElement Schema;
            public string SchemaName;
        }

        private readonly XmlSerializationOption _options;
        private readonly ISchema _schema;
        private Dictionary<Identity, MonikerEntry> _monikers;
        private int _monikerSequence;
        private readonly IDomainModel _domain;
        private IJsonSerializer _serializer;

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
        /// <param name="option">
        ///  (Optional) the option.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public static void Serialize(Stream stream, IDomainModel domain, XmlSerializationOption option = XmlSerializationOption.CompressAll)
        {
            Serialize(stream, domain, new XmlSerializationSettings { Options = option });
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
        /// <param name="settings">
        ///  Options for controlling the operation.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public static void Serialize(Stream stream, IDomainModel domain, XmlSerializationSettings settings)
        {
            Contract.Requires(domain, "domain");

            if (settings == null)
                settings = new XmlSerializationSettings();
            var ser = new XmlSerializer(domain, settings);
            ser.Serialize(stream);
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
        private XmlSerializer(IDomainModel domain, XmlSerializationSettings settings)
        {
            Contract.Requires(domain, "domain");
            _domain = domain;
            _options = settings.Options;
            _schema = settings.Schema;
            _serializer = settings.Serializer;
        }

        private string GetSchemaMoniker(IModelElement mel)
        {
            MonikerEntry moniker;
            if (HasOption(XmlSerializationOption.CompressSchemaId) || HasOption(XmlSerializationOption.Fluent))
            {
                if (_monikers.TryGetValue(mel.SchemaInfo.Id, out moniker))
                    return moniker.Moniker;
            }

            var schemaInfo = GetSchemaInfo(mel, false);

            if (HasOption(XmlSerializationOption.CompressSchemaId) || HasOption(XmlSerializationOption.Fluent))
            {
                MonikerEntry entry;
                if (HasOption(XmlSerializationOption.Fluent))
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

        private bool HasOption(XmlSerializationOption option)
        {
            return (_options & option) == option;
        }

        private void Serialize(Stream stream)
        {
            try
            {
                _monikers = new Dictionary<Identity, MonikerEntry>();

                var entities = SerializeEntities();
                var relationships = SerializeRelationships();
                XElement schemasElement = null;
                if (HasOption(XmlSerializationOption.CompressSchemaId) || HasOption(XmlSerializationOption.Fluent))
                {
                    var schemas = new List<XElement>();
                    foreach (var entries in _monikers.Values.GroupBy(s => s.SchemaName))
                    {
                        var schemaNode = new XElement("schema", new XAttribute("name", entries.Key));
                        schemas.Add(schemaNode);

                        foreach (var entry in entries)
                        {
                            var node = new XElement("add", new XAttribute("name", entry.Schema.Id.Key));
                            schemaNode.Add(node);
                            if (HasOption(XmlSerializationOption.CompressSchemaId))
                                node.Add(new XAttribute("id", entry.Moniker));
                        }
                    }
                    if (schemas.Any())
                    {
                        schemasElement = new XElement("schemas", schemas);
                    }
                }

                using (var writer = XmlWriter.Create(stream))
                {
                    var root = new XElement("domain",
                                    _domain.ExtensionName != null ? new XAttribute("extension", _domain.ExtensionName) : null,
                                    schemasElement,
                                    entities,
                                    relationships); // Order are important

                    root.WriteTo(writer);
                }
            }
            finally
            {
                _monikers = null;
                _monikerSequence = 0;
            }
        }

        private XElement SerializeRelationships()
        {
            var elements = new List<XElement>();
            foreach (var relationship in _domain.GetRelationships())
            {
                XElement node;
                if (HasOption(XmlSerializationOption.Fluent))
                {
                    node = new XElement(GetSchemaMoniker(relationship), new XAttribute("id", GetId(relationship)),
                                  new XAttribute("start", GetId(relationship.Start)), new XAttribute("startSchema", GetSchemaMoniker(relationship.Start)),
                                  new XAttribute("end", GetId(relationship.End)), new XAttribute("endSchema", GetSchemaMoniker(relationship.End)),
                                  SerializeProperties(relationship));
                }
                else
                {
                    node = new XElement("relationship", new XAttribute("id", GetId(relationship)), new XAttribute("schema", GetSchemaMoniker(relationship)),
                                 new XAttribute("start", GetId(relationship.Start)), new XAttribute("startSchema", GetSchemaMoniker(relationship.Start)),
                                 new XAttribute("end", GetId(relationship.End)), new XAttribute("endSchema", GetSchemaMoniker(relationship.End)),
                                 SerializeProperties(relationship));
                }
                elements.Add(node);
            }

            if (!elements.Any())
                return null;

            return new XElement("relationships", elements);
        }

        private string GetId(IModelElement element)
        {
            if (HasOption(XmlSerializationOption.CompressId) && String.Compare(element.Id.DomainModelName, _domain.Name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return element.Id.Key;
            }

            return element.Id.ToString();
        }

        private XElement SerializeEntities()
        {
            var elements = new List<XElement>();
            foreach (var entity in _domain.GetEntities())
            {
                XElement node;
                if (HasOption(XmlSerializationOption.Fluent))
                {
                    node = new XElement(GetSchemaMoniker(entity), new XAttribute("id", GetId(entity)), SerializeProperties(entity));
                }
                else
                {
                    node = new XElement("entity", new XAttribute("id", GetId(entity)), new XAttribute("schema", GetSchemaMoniker(entity)), SerializeProperties(entity));
                }
                elements.Add(node);
            }

            if (!elements.Any())
                return null;

            return new XElement("entities", elements);
        }

        private XElement SerializeProperties(IModelElement element)
        {
            var schemaInfo = GetSchemaInfo(element);
            var elements = new List<XElement>();
            foreach (var prop in schemaInfo.GetProperties(true))
            {
                var value = element.GetPropertyValue(prop);
                if (value.HasValue)
                {
                    XElement node;
                    if (HasOption(XmlSerializationOption.Fluent))
                    {
                        node = new XElement(prop.Name, new XElement("value", prop.Serialize(value.Value, _serializer)));
                    }
                    else
                    {
                        node = new XElement("property", new XAttribute("name", prop.Name), new XElement("value", prop.Serialize(value.Value, _serializer)));

                    }
                    elements.Add(node);
                }
            }
            if (!elements.Any())
                return null;

            return new XElement("properties", elements);
        }
    }
}
