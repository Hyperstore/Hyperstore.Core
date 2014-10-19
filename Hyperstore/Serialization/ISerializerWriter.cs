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
namespace Hyperstore.Modeling.Serialization
{
    struct MonikerEntry
    {
        public string Moniker;
        public ISchemaElement Schema;
        public string SchemaName;
    }

    interface ISerializerWriter
    {
        void NewScope();
        void PushElement(string name, string id, string schemaId, string startId = null, string startSchemaId = null, string endId = null, string endSchemaId = null);
        void PushProperty(string tag, string name, object value);
        void ReduceScope(string tag);
        void SaveTo(System.IO.Stream stream, Hyperstore.Modeling.IDomainModel domain);
        void SaveSchema(System.Collections.Generic.Dictionary<Identity, string> monikers);
    }
}
