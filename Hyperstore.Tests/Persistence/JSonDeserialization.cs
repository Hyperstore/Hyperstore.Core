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
using System.IO;
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Modeling.Serialization;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
    public class JSonDeSerialization : HyperstoreTestBase
    {
        [TestMethod]
        public void SerializeElement()
        {
            var json = "{ \"a\" : 10, \"b\" : \"abcd\" , \"c\" : [ 10.5, null]}";
            JsonReader reader = new JsonReader(new StringReader(json));
            var token = reader.Read();
            Assert.AreEqual(JToken.StartObject, token);
            token = reader.Read();
            Assert.AreEqual(JToken.String, token);
            Assert.AreEqual("a", reader.CurrentValue);
            token = reader.Read();
            Assert.AreEqual(JToken.Colon, token);
            token = reader.Read();
            Assert.AreEqual(JToken.Value, token);
            Assert.AreEqual("10", reader.CurrentValue);
            token = reader.Read();
            Assert.AreEqual(JToken.Comma, token);
            token = reader.Read();
            Assert.AreEqual(JToken.String, token);
            Assert.AreEqual("b", reader.CurrentValue);
            token = reader.Read();
            Assert.AreEqual(JToken.Colon, token);
            token = reader.Read();
            Assert.AreEqual(JToken.String, token);
            Assert.AreEqual("abcd", reader.CurrentValue);

            token = reader.Read();
            Assert.AreEqual(JToken.Comma, token);
            token = reader.Read();
            Assert.AreEqual(JToken.String, token);
            Assert.AreEqual("c", reader.CurrentValue);

            token = reader.Read();
            Assert.AreEqual(JToken.Colon, token);
            token = reader.Read();
            Assert.AreEqual(JToken.StartArray, token);
            token = reader.Read();
            Assert.AreEqual(JToken.Value, token);
            Assert.AreEqual("10.5", reader.CurrentValue);

            token = reader.Read();
            Assert.AreEqual(JToken.Comma, token);
            token = reader.Read();
            Assert.AreEqual(JToken.Value, token);
            Assert.AreEqual("null", reader.CurrentValue);

            token = reader.Read();
            Assert.AreEqual(JToken.EndArray, token);
            token = reader.Read();
            Assert.AreEqual(JToken.EndObject, token);
            token = reader.Read();
            Assert.AreEqual(JToken.EOF, token);
        }
     
    }
}
