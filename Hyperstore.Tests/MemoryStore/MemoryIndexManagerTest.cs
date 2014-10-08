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
    public class MemoryIndexManagerTest : HyperstoreTestBase 
    {
        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task CreateIndex()
        {
            await AssertHelper.ThrowsException<DuplicateIndexException>(async () =>
            {
                // Création concurrente -> Une seule création
                var store = await StoreBuilder.New().CreateAsync();
                var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");
                var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
                MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
                manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");
                manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");
            });
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task IndexExists()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");

            Assert.IsTrue(manager.IndexExists("index1"));
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task GetIndex()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");

            Assert.IsNotNull(manager.GetIndex("index1"));
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task DropIndex()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");
            Assert.IsTrue(manager.IndexExists("index1"));
            manager.DropIndex("index1");
            Assert.IsFalse(manager.IndexExists("index1"));
        }
        
        [TestCategory("MemoryIndexManager")]
        [TestMethod()]
        public async Task WrongIndexNameTest()
        {
            await AssertHelper.ThrowsException<InvalidNameException>(async () =>
            {
                var store = await StoreBuilder.New().CreateAsync();
                var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");
                var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
                MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
                manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index:1", true, "Name");
            });
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task AddToIndex()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            using (var s = store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");

                var index = manager.GetIndex("index1");
                Assert.IsNull(index.Get("momo"));

                var id = new Identity("Test", "1");
                manager.AddToIndex(schema.Definition.XExtendsBaseClass, "index1", id, "momo");

                Assert.AreEqual(id, index.Get("momo"));
            }
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task UniqueConstraintIndex()
        {
            await AssertHelper.ThrowsException<UniqueConstraintException>(async () =>
            {
                var store = await StoreBuilder.New().CreateAsync();
                var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");
                var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
                MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
                using (var s = store.BeginSession(new SessionConfiguration { Readonly = true }))
                {
                    manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");

                    manager.AddToIndex(schema.Definition.XExtendsBaseClass, "index1", new Identity("Test", "1"), "momo");
                    manager.AddToIndex(schema.Definition.XExtendsBaseClass, "index1", new Identity("Test", "2"), "momo");
                }
            });
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task NotUniqueConstraintIndex()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;

            manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", false, "Name");

            manager.AddToIndex(schema.Definition.XExtendsBaseClass, "index1", new Identity("Test", "1"), "momo");
            manager.AddToIndex(schema.Definition.XExtendsBaseClass, "index1", new Identity("Test", "2"), "momo");
            var index = manager.GetIndex("index1");
            using (var session = store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.AreEqual(2, index.GetAll("momo").Count());
                Assert.AreEqual(2, index.GetAll().Count());
            }
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task RemoveFromIndex()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;

            manager.CreateIndex(schema.Definition.XExtendsBaseClass, "index1", true, "Name");

            var index = manager.GetIndex("index1");
            var id = new Identity("Test", "1");
            using (var session = store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                manager.AddToIndex(schema.Definition.XExtendsBaseClass, "index1", id, "momo");
                Assert.AreEqual(id, index.Get("momo"));
                manager.RemoveFromIndex(schema.Definition.XExtendsBaseClass, "index1", id, "momo");
                Assert.IsNull(index.Get("momo"));
            }
        }

    }
}
