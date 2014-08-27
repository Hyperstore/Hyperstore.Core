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
    public class PersistenceTest : HyperstoreTestBase
    {
        [TestMethod]
        public async Task SerializationWithMetadatas()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            CreateGraph(domain);

            using (var writer = new FileStream("test.xml", FileMode.Create))
            {
                var ser = new XmlDomainModelSerializer();
                await ser.Serialize(domain, writer, SerializationOption.All);
            }


            Assert.AreEqual(7, domain.GetEntities().Count());
            Assert.AreEqual(8, domain.GetRelationships().Count());

            store = StoreBuilder.New().Create();

            using (var reader = new FileStream("test.xml", FileMode.Open))
            {
                var ser = new XmlDomainModelSerializer();
                domain = ser.Deserialize(store, reader);
            }

            Assert.AreEqual(7, domain.GetEntities().Count());
            Assert.AreEqual(8, domain.GetRelationships().Count());
        }

        [TestMethod]
        public async Task Serialization()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            CreateGraph(domain);

            using (var writer = new FileStream("test.xml", FileMode.Create))
            {
                var ser = new Hyperstore.Modeling.Serialization.XmlDomainModelSerializer();
                await ser.Serialize(domain, writer, SerializationOption.Elements);
            }
            Assert.AreEqual(7, domain.GetEntities().Count());
            Assert.AreEqual(8, domain.GetRelationships().Count());

            store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            domain = await store.CreateDomainModelAsync("Test");
            using (var reader = new FileStream("test.xml", FileMode.Open))
            {
                var ser = new Hyperstore.Modeling.Serialization.XmlDomainModelSerializer();
                ser.Deserialize(domain, reader);
            }

            Assert.AreEqual(7, domain.GetEntities().Count());
            Assert.AreEqual(8, domain.GetRelationships().Count());
        }

        //[TestMethod]
        //public void SerializationWithProvider()
        //{
        //    if (File.Exists("test2.xml"))
        //        File.Delete("test2.xml");

        //    var store = StoreBuilder.Init().CreateStore();
        //    var domain = store.LoadDomainModel("Test", new TestDomainDefinition()
        //        .Uses<IDomainPersistenceProvider>(r=>new XmlPersistenceProvider("test2.xml"), ModelType.Model)                
        //        ) as TestDomain;

        //    CreateGraph(domain);
        //    Assert.AreEqual(7, domain.GetEntities().Count());
        //    Assert.AreEqual(8, domain.GetRelationships().Count());
        //    store.Close();

        //    store = StoreBuilder.Init().CreateStore();
        //    domain = store.LoadDomainModel("Test", new TestDomainDefinition()
        //        .Uses<IDomainPersistenceProvider>(r => new XmlPersistenceProvider("test2.xml"), ModelType.Model)
        //        ) as TestDomain;

        //    Assert.AreEqual(7, domain.GetEntities().Count());
        //    Assert.AreEqual(8, domain.GetRelationships().Count());
        //}

        //[TestMethod]
        //public void SerializationSaveOnUnload()
        //{
        //    if (File.Exists("test2.xml"))
        //        File.Delete("test2.xml");

        //    var store = StoreBuilder.Init().CreateStore();
        //    var domain = store.LoadDomainModel("Test", new TestDomainDefinition()
        //        .Uses<IDomainPersistenceProvider>(r => new XmlPersistenceProvider("test2.xml"), ModelType.Model)
        //        ) as TestDomain;

        //    CreateGraph(domain);
        //    Assert.AreEqual(7, domain.GetEntities().Count());
        //    Assert.AreEqual(8, domain.GetRelationships().Count());
        //    store.UnloadDomainOrExtension(domain);

        //    store = StoreBuilder.Init().CreateStore();
        //    domain = store.LoadDomainModel("Test", new TestDomainDefinition()
        //        .Uses<IDomainPersistenceProvider>(r => new XmlPersistenceProvider("test2.xml"), ModelType.Model)
        //        ) as TestDomain;

        //    Assert.AreEqual(7, domain.GetEntities().Count());
        //    Assert.AreEqual(8, domain.GetRelationships().Count());
        //}
    }
}
