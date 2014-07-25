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
            var store = new Store();
            var schema = await store.LoadSchemaAsync(new TestDomainDefinition());
            var dm = await store.CreateDomainModelAsync("Test");

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
