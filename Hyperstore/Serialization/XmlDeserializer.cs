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
    ///  A son serialization settings.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class XmlDeserializationSettings
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
        public XmlDeserializationSettings()
        {
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A son domain model serializer.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public partial class XmlDeserializer
    {
        private readonly ISchema _schema;
        private Dictionary<string, ISchemaElement> _monikers;
        private readonly IDomainModel _domain;
        private IJsonSerializer _serializer;
        private XmlReader _reader;

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
        ///  Options for controlling the operation.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public static void Deserialize(Stream stream, IDomainModel domain, XmlDeserializationSettings settings = null)
        {
            Contract.Requires(domain, "domain");
            var ser = new XmlDeserializer(domain, settings);
            ser.Deserialize(stream);
        }


        #endregion

        private XmlDeserializer(IDomainModel domain, XmlDeserializationSettings settings)
        {
            _domain = domain;
            if (settings != null)
            {
                _serializer = settings.Serializer;
                _schema = settings.Schema;
            }
        }

        private ISchemaElement GetSchemaFromMoniker(string moniker)
        {
            ISchemaElement schema;
            if (_monikers != null)
            {
                if (_monikers.TryGetValue(moniker, out schema))
                    return schema;
            }

            schema = GetSchemaInfo(moniker);
            _monikers.Add(moniker, schema);
            return schema;
        }

        private string ReadNextElement()
        {
            do
            {
                if (!_reader.Read())
                    return null;
            }
            while (_reader.NodeType != XmlNodeType.Element);

            return _reader.LocalName;
        }

        private Identity ReadId(string name)
        {
            var value = ReadAttribute(name);
            if (value.IndexOf(':') > 0)
                return Identity.Parse(value);
            return _domain.CreateId(value);
        }

        private string ReadAttribute(string name, bool throwException = true)
        {
            var value = _reader.GetAttribute(name);
            if (value == null && throwException)
                throw new Exception(String.Format("Invalid file format. Expected attribute {0} in element {1}", name, _reader.LocalName));
            return value;
        }


        private void Deserialize(Stream stream)
        {
            _monikers = new Dictionary<string, ISchemaElement>();
            try
            {
                using (_reader = XmlReader.Create(stream))
                {
                    using (var session = _domain.Store.BeginSession(new SessionConfiguration { Mode = SessionMode.Loading }))
                    {
                        var elem = ReadNextElement();
                        if (elem != "domain")
                            throw new Exception("Invalid format file");

                        elem = ReadNextElement();
                        if (elem == "schemas")
                        {
                            while ((elem = ReadNextElement()) == "schema")
                            {
                                var name = ReadAttribute("name");
                                var moniker = ReadAttribute("id");
                                var schema = GetSchemaInfo(name);
                                _monikers.Add(moniker, schema);
                            }
                        }

                        ReadEntities();
                        ReadRelationships();

                        session.AcceptChanges();
                    }
                }
            }
            finally
            {
                _monikers = null;
            }
        }

        private ISchemaElement GetSchemaInfo(string moniker)
        {
            var schema = _schema != null ? _schema.GetSchemaElement(moniker) : _domain.Store.GetSchemaElement(moniker);
            return schema;
        }

        private void ReadRelationships()
        {
            if (_reader.LocalName == "relationships")
            {
                string elem = ReadNextElement();
                while (elem == "relationship")
                {
                    var metadata = ReadAttribute("schema");
                    var id = ReadId("id");
                    var schema = GetSchemaFromMoniker(metadata) as ISchemaRelationship;
                    if (schema == null)
                        throw new Exception(String.Format("Invalid metadata {0} for relationship {1}", metadata, id));

                    var smetadata = ReadAttribute("startSchema");
                    var startId = ReadId("start");
                    var startSchema = GetSchemaFromMoniker(smetadata);
                    if (startSchema == null)
                        throw new Exception(String.Format("Invalid start metadata {0} for relationship {1}", smetadata, id));

                    var emetadata = ReadAttribute("endSchema");
                    var endId = ReadId("end");
                    var endSchema = GetSchemaFromMoniker(emetadata);
                    if (endSchema == null)
                        throw new Exception(String.Format("Invalid end metadata {0} for relationship {1}", emetadata, id));

                    var entity = _domain.CreateRelationship(schema, startId, startSchema, endId, endSchema, id);

                    elem = ReadProperties(entity, schema);
                }
            }
        }

        private void ReadEntities()
        {
            if (_reader.LocalName == "entities")
            {
                string elem = ReadNextElement();
                while (elem == "entity")
                {
                    var metadata = ReadAttribute("schema");
                    var id = ReadId("id");
                    var schema = GetSchemaFromMoniker(metadata) as ISchemaEntity;
                    if (schema == null)
                        throw new Exception(String.Format("Invalid metadata {0} for entity {1}", metadata, id));

                    var entity = _domain.CreateEntity(schema, id);

                    elem = ReadProperties(entity, schema);
                }
            }
        }

        private string ReadProperties(IModelElement element, ISchemaElement schema)
        {
            var elem = ReadNextElement();
            if (elem == "properties")
            {
                while ((elem = ReadNextElement()) == "property")
                {
                    var name = ReadAttribute("name");
                    var prop = schema.GetProperty(name);
                    if (prop == null)
                        throw new Exception(String.Format("Unknow value property {1} for element {1}", name, element.Id));

                    var vElem = ReadNextElement();
                    if (vElem != "value")
                        throw new Exception(String.Format("Value expected for property {1} of element {1}", name, element.Id));
                    
                    _reader.Read();
                    var cmd = new Hyperstore.Modeling.Commands.ChangePropertyValueCommand(element, prop, prop.PropertySchema.Deserialize(new SerializationContext(prop, _reader.Value)));
                    Session.Current.Execute(cmd);
                }
            }
            return elem;
        }
    }
}
