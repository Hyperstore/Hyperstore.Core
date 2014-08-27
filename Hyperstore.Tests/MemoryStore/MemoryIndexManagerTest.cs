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
                var store = StoreBuilder.New().Create();
                await store.LoadSchemaAsync(new TestDomainDefinition());
                var domain = await store.CreateDomainModelAsync("Test");
                var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
                MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
                manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
                manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
            });
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task IndexExists()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");

            Assert.IsTrue(manager.IndexExists("index1"));
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task GetIndex()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");

            Assert.IsNotNull(manager.GetIndex("index1"));
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task DropIndex()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
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
                var store = StoreBuilder.New().Create();
                await store.LoadSchemaAsync(new TestDomainDefinition());
                var domain = await store.CreateDomainModelAsync("Test");
                var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
                MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
                manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index:1", true, "Name");
            });
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task AddToIndex()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
            using (var s = store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");

                var index = manager.GetIndex("index1");
                Assert.IsNull(index.Get("momo"));

                var id = new Identity("Test", "1");
                manager.AddToIndex(TestDomainDefinition.XExtendsBaseClass, "index1", id, "momo");

                Assert.AreEqual(id, index.Get("momo"));
            }
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task UniqueConstraintIndex()
        {
            await AssertHelper.ThrowsException<UniqueConstraintException>(async () =>
            {
                var store = StoreBuilder.New().Create();
                await store.LoadSchemaAsync(new TestDomainDefinition());
                var domain = await store.CreateDomainModelAsync("Test");
                var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
                MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;
                using (var s = store.BeginSession(new SessionConfiguration { Readonly = true }))
                {
                    manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");

                    manager.AddToIndex(TestDomainDefinition.XExtendsBaseClass, "index1", new Identity("Test", "1"), "momo");
                    manager.AddToIndex(TestDomainDefinition.XExtendsBaseClass, "index1", new Identity("Test", "2"), "momo");
                }
            });
        }

        [TestMethod]
        [TestCategory("MemoryIndexManager")]
        public async Task NotUniqueConstraintIndex()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;

            manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", false, "Name");

            manager.AddToIndex(TestDomainDefinition.XExtendsBaseClass, "index1", new Identity("Test", "1"), "momo");
            manager.AddToIndex(TestDomainDefinition.XExtendsBaseClass, "index1", new Identity("Test", "2"), "momo");
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
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = domain.Resolve<IHyperGraph>() as Hyperstore.Modeling.HyperGraph.HyperGraph;
            MemoryIndexManager manager = graph.IndexManager as MemoryIndexManager;

            manager.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");

            var index = manager.GetIndex("index1");
            var id = new Identity("Test", "1");
            using (var session = store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                manager.AddToIndex(TestDomainDefinition.XExtendsBaseClass, "index1", id, "momo");
                Assert.AreEqual(id, index.Get("momo"));
                manager.RemoveFromIndex(TestDomainDefinition.XExtendsBaseClass, "index1", id, "momo");
                Assert.IsNull(index.Get("momo"));
            }
        }

    }
}
