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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Serialization
{
    class JSonDomainModelDeserializer
    {
        public static void Deserialize(IDomainModel domain, string json)
        {
            Contract.Requires(domain, "domain");
            if (string.IsNullOrEmpty(json))
                return;

            var ser = new JSonDomainModelDeserializer();
            ser.Deserialize(json);
        }

        private JsonReader _reader;
        private JToken _currentToken;
        private IDomainModel _domain;

        public JSonDomainModelDeserializer()
        {
        }

        private void Deserialize(string json)
        {
            using (var sw = new StringReader(json))
            {
                try
                {
                    _reader = new JsonReader(sw);
                    _reader.Read();
                    ReadElement();
                }
                finally
                {
                    _reader = null;
                }
            }
        }

        private void ReadElement()
        {
            Accept(JToken.StartObject);

            var id = ReadTechnicalProperty("_id");
            Accept(JToken.Comma);
            var sid = ReadTechnicalProperty("_shid");

            IModelElement element;
            var schema = _domain.Store.GetSchemaInfo(sid, true);
            if( schema is ISchemaRelationship)
            {
                var startId = ReadTechnicalProperty("_sid");
                var startSchemaId = ReadTechnicalProperty("_sshid");
                var endId = ReadTechnicalProperty("_eid");
                var endSchemaId = ReadTechnicalProperty("_eshid");

                element = _domain.CreateRelationship(schema as ISchemaRelationship, startId, _domain.Store.GetSchemaElement(startSchemaId, true), endId, _domain.Store.GetSchemaElement(endSchemaId, true), id); 
            }
            else
            {
                element = _domain.CreateEntity(schema as ISchemaEntity, id);
            }

            Accept(JToken.EndObject);
        }

        private Identity ReadTechnicalProperty(string expectedKey)
        {
            var key = _reader.CurrentValue;
            Accept(JToken.String);
            if (key != expectedKey)
                throw Error("Invalid data for entity - Expected {0} property.", expectedKey);
            Accept(JToken.Colon);
            var value = _reader.CurrentValue;
            Accept(JToken.String);
            Identity id;
            if (!Identity.TryParse(value, out id))
                throw Error("Invalid identity for {0}", expectedKey);
            return id;
        }

        private void Accept(JToken expectedToken)
        {
            if( _currentToken != expectedToken)   
                throw Error("Incorrect JSON format. Expected {0}", expectedToken);
            _currentToken = _reader.Read();
        }

        private Exception Error(string format, params object[] args)
        {
            return new Exception(String.Format("Error at position {0} - {1}", _reader.CurrentPos, String.Format(format, args)));
        }
    }
}
