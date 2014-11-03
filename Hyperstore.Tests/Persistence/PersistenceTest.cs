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
using System.IO;
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Modeling.Serialization;
using Hyperstore.Tests.Model;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{

    public class PersistenceTest : HyperstoreTestBase
    {
        [Fact]
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


            var text = Hyperstore.Modeling.Serialization.HyperstoreSerializer.Serialize(domain);
            Assert.Equal(11, domain.GetEntities().Count());
            Assert.Equal(10, domain.GetRelationships().Count());

            var store2 = await StoreBuilder.New().CreateAsync();
            await store2.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain2 = await store2.DomainModels.New().CreateAsync("Test");
            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                Hyperstore.Modeling.Serialization.XmlDeserializer.Deserialize(reader, domain2);
            }

            Assert.Equal(11, domain2.GetEntities().Count());
            Assert.Equal(10, domain2.GetRelationships().Count());

            foreach (var book2 in domain2.GetEntities<Book>())
            {
                var book = domain.GetEntity<Book>(((IModelElement)book2).Id);
                Assert.NotNull(book);
                Assert.Equal(book.Title, book2.Title);
                Assert.Equal(book.Copies, book2.Copies);
            }

            lib = domain2.GetEntities<Library>().FirstOrDefault();
            Assert.NotNull(lib);
            Assert.Equal(5, lib.Books.Count());
        }

        [Fact]
        public async Task JsonSerialization()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib1";
                for (int i = 0; i < 1; i++)
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

            var text = Hyperstore.Modeling.Serialization.HyperstoreSerializer.Serialize(domain, new SerializationSettings { Options = SerializationOptions.Json | SerializationOptions.CompressSchema });
            Assert.NotNull(text);
            Newtonsoft.Json.JsonConvert.DeserializeObject(text);
        }

        [Fact]
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

            var text = Hyperstore.Modeling.Serialization.HyperstoreSerializer.Serialize(domain);
            Assert.Equal(11, domain.GetEntities().Count());
            Assert.Equal(10, domain.GetRelationships().Count());

            var store2 = await StoreBuilder.New().CreateAsync();
            await store2.Schemas.New<LibraryDefinition>().CreateAsync();

            // New domain name
            var domain2 = await store2.DomainModels.New().CreateAsync("Test2");
            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                Hyperstore.Modeling.Serialization.XmlDeserializer.Deserialize(reader, domain2);
            }

            Assert.Equal(11, domain2.GetEntities().Count());
            Assert.Equal(10, domain2.GetRelationships().Count());

            IModelElement library = domain2.GetEntities<Library>().FirstOrDefault();
            Assert.NotNull(library);
            Assert.Equal("test2", library.Id.DomainModelName);
        }

        [Fact]
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

            for (int i = 0; i < 40; i++)
            {
                var domain2 = await store.DomainModels.New().CreateAsync("Test2");
                var text = Hyperstore.Modeling.Serialization.HyperstoreSerializer.Serialize(domain2);
                store.DomainModels.Unload(domain2);
            }
            var time2 = sw.ElapsedMilliseconds;
            Assert.True(time2 < 800);
        }


    }
}
