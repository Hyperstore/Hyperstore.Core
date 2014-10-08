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
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Memory
{
    [TestClass()]
    public class MemoryIndexTest : Hyperstore.Tests.HyperstoreTestBase
    {
        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task CreateIndexDefinition()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = ((Hyperstore.Modeling.Domain.IHyperGraphProvider)domain).InnerGraph;

            var def = new IndexDefinition(graph, "Index1", schema.Definition.XExtendsBaseClass, true, "Name");
            Assert.AreEqual(1, def.PropertyNames.Length);
            Assert.AreEqual("Name", def.PropertyNames[0]);
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task IsImpactedIndexDefinition()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = ((Hyperstore.Modeling.Domain.IHyperGraphProvider)domain).InnerGraph;
            var def = new IndexDefinition(graph, "Index1", schema.Definition.XExtendsBaseClass, true, "Name");
            Assert.IsTrue(def.IsImpactedBy(schema.Definition.XExtendsBaseClass, "Name"));
            Assert.IsFalse(def.IsImpactedBy(schema.Definition.XExtendsBaseClass, "Name2"));
            Assert.IsTrue(def.IsImpactedBy(schema.Definition.XExtendsBaseClass, null));
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task AddInTransaction()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            domain.Indexes.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");
            var index = domain.Indexes.GetIndex("index1");

            using (var s = domain.Store.BeginSession())
            {
                dynamic a = new DynamicModelEntity(domain, schema.Definition.XExtendsBaseClass);
                a.Name = "momo";
                s.AcceptChanges();
                // Est visible dans la transaction
                Assert.IsNotNull(domain.GetElement(index.Get("momo"), schema.Definition.XExtendsBaseClass));
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNotNull(domain.GetElement(index.Get("momo"), schema.Definition.XExtendsBaseClass));
            }
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task UpdateIndexedProperty()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            domain.Indexes.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");
            var index = domain.Indexes.GetIndex("index1");

            using (var s = domain.Store.BeginSession())
            {
                dynamic a = new DynamicModelEntity(domain, schema.Definition.XExtendsBaseClass);
                a.Name = "momo";
                s.AcceptChanges();
            }

            dynamic mel;
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                mel = domain.GetElement(index.Get("momo"), schema.Definition.XExtendsBaseClass);
                Assert.IsNotNull(mel);
            }
            using (var s = domain.Store.BeginSession())
            {
                mel.Name = "mama";
                s.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNull(domain.GetElement(index.Get("momo"), schema.Definition.XExtendsBaseClass));
                Assert.IsNotNull(domain.GetElement(index.Get("mama"), schema.Definition.XExtendsBaseClass));
            }
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task RemoveIndexElement()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            domain.Indexes.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");
            domain.Indexes.CreateIndex(schema.Definition.XExtendsBaseClass, "index2", true, "Value");
            var index1 = domain.Indexes.GetIndex("index1");
            var index2 = domain.Indexes.GetIndex("index2");

            using (var s = domain.Store.BeginSession())
            {
                dynamic a = new DynamicModelEntity(domain, schema.Definition.XExtendsBaseClass);
                a.Name = "momo";
                a.Value = 10;
                s.AcceptChanges();
            }

            dynamic mel = domain.GetElement(index1.Get("momo"), schema.Definition.XExtendsBaseClass);
            Assert.IsNotNull(mel);

            mel = domain.GetElement(index2.Get(10), schema.Definition.XExtendsBaseClass);
            Assert.IsNotNull(mel);

            using (var s = domain.Store.BeginSession())
            {
                s.Execute(new RemoveEntityCommand(mel));
                s.AcceptChanges();

                // Ne doit plus être visible ds la transaction
                Assert.IsNull(domain.GetElement(index1.Get("momo"), schema.Definition.XExtendsBaseClass));
                Assert.IsNull(domain.GetElement(index2.Get(10), schema.Definition.XExtendsBaseClass));
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNull(domain.GetElement(index1.Get("momo"), schema.Definition.XExtendsBaseClass));
                Assert.IsNull(domain.GetElement(index2.Get(10), schema.Definition.XExtendsBaseClass));
            }
        }
    }
}
