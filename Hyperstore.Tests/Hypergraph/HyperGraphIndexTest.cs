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
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            domain.Indexes.CreateIndex(TestDomainDefinition.XExtendsBaseClass, "index1", true, "Name");
        }

        [TestCategory("Hypergraph")]
        [TestMethod()]
        public async Task IndexTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

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
