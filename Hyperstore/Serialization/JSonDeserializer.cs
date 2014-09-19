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

            var ser = new JSonDomainModelDeserializer(domain);
            ser.Deserialize(json);
        }

        private JsonReader _reader;
        private JToken _currentToken;
        private IDomainModel _domain;

        public JSonDomainModelDeserializer(IDomainModel domain)
        {
            Contract.Requires(domain, "domain");
            _domain = domain;
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
