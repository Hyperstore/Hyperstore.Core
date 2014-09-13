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
        CompressId=2,

        /// <summary>
        ///  Specifies the compress all= 3 option.
        /// </summary>
        CompressAll=3
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
        /// <param name="settings">
        ///  Options for controlling the operation.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public XmlSerializer(IDomainModel domain, XmlSerializationSettings settings)
        {
            _domain = domain;
            _options = settings.Options;
            _schema = settings.Schema;
            _serializer = settings.Serializer;
        }

        private string GetSchemaMoniker(IModelElement mel)
        {
            MonikerEntry moniker;
            if (_monikers != null)
            {
                if (_monikers.TryGetValue(mel.SchemaInfo.Id, out moniker))
                    return moniker.Moniker;
            }

            var schemaInfo = GetSchemaInfo(mel, false);
            if (_monikers != null)
            {
                MonikerEntry entry;
                _monikerSequence++;
                entry.Moniker = _monikerSequence.ToString();
                entry.Schema = schemaInfo;
                _monikers[mel.SchemaInfo.Id] = entry;
                return entry.Moniker;
            }

            return schemaInfo.Id.ToString();
        }

        private ISchemaElement GetSchemaInfo(IModelElement mel, bool findInMoniker=false)
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Entry point
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        private void Serialize(Stream stream)
        {
            try
            {
                if (HasOption(XmlSerializationOption.CompressSchemaId))
                {
                    _monikers = new Dictionary<Identity, MonikerEntry>();
                }

                var entities = SerializeEntities();
                var relationships = SerializeRelationships();
                XElement schemasElement = null;
                if (HasOption(XmlSerializationOption.CompressSchemaId))
                {
                    var schemas = new List<XElement>();
                    foreach (var entry in _monikers.Values)
                    {
                        schemas.Add(new XElement("schema", new XAttribute("id", entry.Moniker), new XAttribute("name", entry.Schema.Id.ToString())));
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
                elements.Add(new XElement("relationship", new XAttribute("id", GetId(relationship)), new XAttribute("schema", GetSchemaMoniker(relationship)),
                             new XAttribute("start", GetId(relationship.Start)), new XAttribute("startSchema", GetSchemaMoniker(relationship.Start)),
                             new XAttribute("end", GetId(relationship.End)), new XAttribute("endSchema", GetSchemaMoniker(relationship.End)),
                             SerializeProperties(relationship)));
            }

            if (!elements.Any())
                return null;

            return new XElement("relationships", elements);
        }

        private string GetId(IModelElement element)
        {
            if( HasOption( XmlSerializationOption.CompressId) && String.Compare(element.Id.DomainModelName, _domain.Name)==0)
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
                elements.Add(new XElement("entity", new XAttribute("id", GetId(entity)), new XAttribute("schema", GetSchemaMoniker(entity)), SerializeProperties(entity)));
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
                    elements.Add(new XElement("property", new XAttribute("name", prop.Name), new XElement("value", prop.Serialize(value.Value, _serializer))));
                }
            }
            if (!elements.Any())
                return null;

            return new XElement("properties", elements);
        }
    }
}
