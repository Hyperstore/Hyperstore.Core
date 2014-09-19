//	Copyright ? 2013 - 2014, Alain Metge. All rights reserved.
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
 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Hyperstore.Modeling;
using System.Diagnostics;
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Metadata;
using System.Globalization;
using Hyperstore.Tests.Model;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Model
{
    partial class XExtendsBaseClass
    {
        public int CalculatedValue
        {
            get
            {
                return CalculatedProperty(() => Value * 5);
            }
        }
    }
}

namespace Hyperstore.Tests
{
    [TestClass]
    public class PropertiesTest : HyperstoreTestBase
    {
        [TestMethod]
        public async Task CalculatedPropertyTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            XExtendsBaseClass start = null;
            using( var s = store.BeginSession() )
            {
                start = new XExtendsBaseClass(dm);
                Assert.AreEqual(0, start.CalculatedValue);
                s.AcceptChanges();
            }

            bool flag=false;
            start.PropertyChanged += (sender, e) => { if (e.PropertyName == "CalculatedValue") flag = true; };

            using (var s = store.BeginSession())
            {
                start.Value = 10;
                s.AcceptChanges();
            }

            Assert.IsTrue(flag);
            Assert.AreEqual(50, start.CalculatedValue);

        
        }
    }
}
