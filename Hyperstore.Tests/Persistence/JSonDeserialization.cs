//	Copyright � 2013 - 2014, Alain Metge. All rights reserved.
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
using System.IO;
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Modeling.Serialization;
using Hyperstore.Tests.Model;
using Xunit;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    
    public class JSonDeSerialization : HyperstoreTestBase
    {
        [Fact]
        public void SerializeJsonElement()
        {
            var json = "{ \"a\" : 10, \"b\" : \"abcd\" , \"c\" : [ 10.5, null]}";
            JsonReader reader = new JsonReader(new StringReader(json));
            var token = reader.Read();
            Assert.Equal(JToken.StartObject, token);
            token = reader.Read();
            Assert.Equal(JToken.String, token);
            Assert.Equal("a", reader.CurrentValue);
            token = reader.Read();
            Assert.Equal(JToken.Colon, token);
            token = reader.Read();
            Assert.Equal(JToken.Value, token);
            Assert.Equal("10", reader.CurrentValue);
            token = reader.Read();
            Assert.Equal(JToken.Comma, token);
            token = reader.Read();
            Assert.Equal(JToken.String, token);
            Assert.Equal("b", reader.CurrentValue);
            token = reader.Read();
            Assert.Equal(JToken.Colon, token);
            token = reader.Read();
            Assert.Equal(JToken.String, token);
            Assert.Equal("abcd", reader.CurrentValue);

            token = reader.Read();
            Assert.Equal(JToken.Comma, token);
            token = reader.Read();
            Assert.Equal(JToken.String, token);
            Assert.Equal("c", reader.CurrentValue);

            token = reader.Read();
            Assert.Equal(JToken.Colon, token);
            token = reader.Read();
            Assert.Equal(JToken.StartArray, token);
            token = reader.Read();
            Assert.Equal(JToken.Value, token);
            Assert.Equal("10.5", reader.CurrentValue);

            token = reader.Read();
            Assert.Equal(JToken.Comma, token);
            token = reader.Read();
            Assert.Equal(JToken.Value, token);
            Assert.Equal("null", reader.CurrentValue);

            token = reader.Read();
            Assert.Equal(JToken.EndArray, token);
            token = reader.Read();
            Assert.Equal(JToken.EndObject, token);
            token = reader.Read();
            Assert.Equal(JToken.EOF, token);
        }
     
    }
}
