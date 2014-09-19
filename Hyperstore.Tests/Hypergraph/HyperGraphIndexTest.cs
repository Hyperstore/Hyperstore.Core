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
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Hypergraph
{
    [TestClass()]
    public class HyperGraphIndexTest : HyperstoreTestBase
    {
        [TestCategory("Hypergraph")]
        [TestMethod()]
        public async Task CreateIndexTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            domain.Indexes.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
        }

        [TestCategory("Hypergraph")]
        [TestMethod()]
        public async Task IndexTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            domain.Indexes.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");

            using (var tx = domain.Store.BeginSession())
            {
                dynamic a = new DynamicModelEntity(domain, TestDomainDefinition.XExtendsBaseClass);
                a.Name = "toto";
                tx.AcceptChanges();
            }
        }
    }
}
