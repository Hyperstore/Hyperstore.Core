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
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = ((Hyperstore.Modeling.Domain.IHyperGraphProvider)domain).InnerGraph;

            var def = new IndexDefinition(graph, "Index1", TestDomainDefinition.XExtendsBaseClass, true, "Name");
            Assert.AreEqual(1, def.PropertyNames.Length);
            Assert.AreEqual("Name", def.PropertyNames[0]);
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task IsImpactedIndexDefinition()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var graph = ((Hyperstore.Modeling.Domain.IHyperGraphProvider)domain).InnerGraph;
            var def = new IndexDefinition(graph, "Index1", TestDomainDefinition.XExtendsBaseClass, true, "Name");
            Assert.IsTrue(def.IsImpactedBy(TestDomainDefinition.XExtendsBaseClass, "Name"));
            Assert.IsFalse(def.IsImpactedBy(TestDomainDefinition.XExtendsBaseClass, "Name2"));
            Assert.IsTrue(def.IsImpactedBy(TestDomainDefinition.XExtendsBaseClass, null));
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task AddInTransaction()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            domain.Indexes.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
            var index = domain.Indexes.GetIndex("index1");

            using (var s = domain.Store.BeginSession())
            {
                dynamic a = new DynamicModelEntity(domain, TestDomainDefinition.XExtendsBaseClass);
                a.Name = "momo";
                s.AcceptChanges();
                // Est visible dans la transaction
                Assert.IsNotNull(domain.GetElement(index.Get("momo"), TestDomainDefinition.XExtendsBaseClass));
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNotNull(domain.GetElement(index.Get("momo"), TestDomainDefinition.XExtendsBaseClass));
            }
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task UpdateIndexedProperty()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            domain.Indexes.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
            var index = domain.Indexes.GetIndex("index1");

            using (var s = domain.Store.BeginSession())
            {
                dynamic a = new DynamicModelEntity(domain, TestDomainDefinition.XExtendsBaseClass);
                a.Name = "momo";
                s.AcceptChanges();
            }

            dynamic mel;
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                mel = domain.GetElement(index.Get("momo"), TestDomainDefinition.XExtendsBaseClass);
                Assert.IsNotNull(mel);
            }
            using (var s = domain.Store.BeginSession())
            {
                mel.Name = "mama";
                s.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNull(domain.GetElement(index.Get("momo"), TestDomainDefinition.XExtendsBaseClass));
                Assert.IsNotNull(domain.GetElement(index.Get("mama"), TestDomainDefinition.XExtendsBaseClass));
            }
        }

        [TestMethod]
        [TestCategory("MemoryIndex")]
        public async Task RemoveIndexElement()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            domain.Indexes.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
            domain.Indexes.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index2", true, "Value");
            var index1 = domain.Indexes.GetIndex("index1");
            var index2 = domain.Indexes.GetIndex("index2");

            using (var s = domain.Store.BeginSession())
            {
                dynamic a = new DynamicModelEntity(domain, TestDomainDefinition.XExtendsBaseClass);
                a.Name = "momo";
                a.Value = 10;
                s.AcceptChanges();
            }

            dynamic mel = domain.GetElement(index1.Get("momo"), TestDomainDefinition.XExtendsBaseClass);
            Assert.IsNotNull(mel);

            mel = domain.GetElement(index2.Get(10), TestDomainDefinition.XExtendsBaseClass);
            Assert.IsNotNull(mel);

            using (var s = domain.Store.BeginSession())
            {
                s.Execute(new RemoveEntityCommand(mel));
                s.AcceptChanges();

                // Ne doit plus être visible ds la transaction
                Assert.IsNull(domain.GetElement(index1.Get("momo"), TestDomainDefinition.XExtendsBaseClass));
                Assert.IsNull(domain.GetElement(index2.Get(10), TestDomainDefinition.XExtendsBaseClass));
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNull(domain.GetElement(index1.Get("momo"), TestDomainDefinition.XExtendsBaseClass));
                Assert.IsNull(domain.GetElement(index2.Get(10), TestDomainDefinition.XExtendsBaseClass));
            }
        }
    }
}
