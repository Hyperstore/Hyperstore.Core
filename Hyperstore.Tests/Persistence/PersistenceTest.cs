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
using System.Diagnostics;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
    public class PersistenceTest : HyperstoreTestBase
    {
        [TestMethod]
        public async Task Serialization()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib1";
                for (int i = 0; i < 5; i++)
                {
                    var b = new Book(domain);
                    b.Title = "Book \"book\" " + i.ToString();
                    b.Copies = i + 1;
                    lib.Books.Add(b);

                    var m = new Member(domain);
                    m.Name = "Book " + i.ToString();
                    lib.Members.Add(m);

                }
                session.AcceptChanges();
            }

            using (var writer = new FileStream("test.xml", FileMode.Create))
            {
                Hyperstore.Modeling.Serialization.XmlSerializer.Serialize(writer, domain, XmlSerializationOption.CompressAll);
            }
            Assert.AreEqual(11, domain.GetEntities().Count());
            Assert.AreEqual(10, domain.GetRelationships().Count());

            var store2 = await StoreBuilder.New().CreateAsync();
            await store2.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain2 = await store2.DomainModels.New().CreateAsync("Test");
            using (var reader = new FileStream("test.xml", FileMode.Open))
            {
                Hyperstore.Modeling.Serialization.XmlDeserializer.Deserialize(reader, domain2);
            }

            Assert.AreEqual(11, domain2.GetEntities().Count());
            Assert.AreEqual(10, domain2.GetRelationships().Count());

            foreach(var book2 in domain2.GetEntities<Book>())
            {
                var book = domain.GetEntity<Book>(((IModelElement)book2).Id);
                Assert.IsNotNull(book);
                Assert.AreEqual(book.Title, book2.Title);
                Assert.AreEqual(book.Copies, book2.Copies);
            }

            lib = domain2.GetEntities<Library>().FirstOrDefault();
            Assert.IsNotNull(lib);
            Assert.AreEqual(5, lib.Books.Count());
        }

        [TestMethod]
        public async Task SerializeAndDeserializeWithNewDomainName()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib1";
                for (int i = 0; i < 5; i++)
                {
                    var b = new Book(domain);
                    b.Title = "Book \"book\" " + i.ToString();
                    b.Copies = i + 1;
                    lib.Books.Add(b);

                    var m = new Member(domain);
                    m.Name = "Book " + i.ToString();
                    lib.Members.Add(m);

                }
                session.AcceptChanges();
            }

            using (var writer = new FileStream("test.xml", FileMode.Create))
            {
                Hyperstore.Modeling.Serialization.XmlSerializer.Serialize(writer, domain, XmlSerializationOption.CompressAll);
            }
            Assert.AreEqual(11, domain.GetEntities().Count());
            Assert.AreEqual(10, domain.GetRelationships().Count());

            var store2 = await StoreBuilder.New().CreateAsync();
            await store2.Schemas.New<LibraryDefinition>().CreateAsync();

            // New domain name
            var domain2 = await store2.DomainModels.New().CreateAsync("Test2");
            using (var reader = new FileStream("test.xml", FileMode.Open))
            {
                Hyperstore.Modeling.Serialization.XmlDeserializer.Deserialize(reader, domain2);
            }

            Assert.AreEqual(11, domain2.GetEntities().Count());
            Assert.AreEqual(10, domain2.GetRelationships().Count());

            IModelElement library = domain2.GetEntities<Library>().FirstOrDefault();            
            Assert.IsNotNull(library);
            Assert.AreEqual("test2", library.Id.DomainModelName);            
        }

        [TestMethod]
        public async Task SerializePerf()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib1";
                for (int i = 0; i < 5; i++)
                {
                    var b = new Book(domain);
                    b.Title = "Book \"book\" " + i.ToString();
                    b.Copies = i + 1;
                    lib.Books.Add(b);

                    var m = new Member(domain);
                    m.Name = "Book " + i.ToString();
                    lib.Members.Add(m);

                }
                session.AcceptChanges();
            }

            var sw = new Stopwatch();
            sw.Start();
            var size1 = 0L;
            for (int i = 0; i < 40; i++)
            {
                using (var writer = new FileStream("test2.xml", FileMode.Create))
                {
                    var domain2 = await store.DomainModels.New().CreateAsync("Test2");
                    var ser = new XmlDomainModelSerializer();
                    await ser.Serialize(domain2, writer, SerializationOption.Elements);
                    store.DomainModels.Unload(domain2);
                    size1 += writer.Length; 
                }
            }
            var time1 = sw.ElapsedMilliseconds;

            sw.Restart();
            var size2 = 0L;
            for (int i = 0; i < 40; i++)
            {
                using (var writer = new FileStream("test2.xml", FileMode.Create))
                {
                    var domain2 = await store.DomainModels.New().CreateAsync("Test2");
                    Hyperstore.Modeling.Serialization.XmlSerializer.Serialize(writer, domain2, XmlSerializationOption.CompressAll);
                    store.DomainModels.Unload(domain2);
                    size2 += writer.Length;
                }
            }
            var time2 = sw.ElapsedMilliseconds;
            Assert.IsTrue(time2 < time1, String.Format("XmlDomainSerializer should have an execution time {0} greater than XmlSerializer {1}", time1, time2));
            Assert.IsTrue(size2 < size1, String.Format("XmlDomainSerializer should have a generated size file {0} greater than XmlSerializer {1}", size1, size2));
        }

    
    }
}
