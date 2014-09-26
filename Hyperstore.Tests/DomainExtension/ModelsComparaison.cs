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

using Hyperstore.Modeling;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Serialization;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Tests.DomainExtension
{
    [TestClass()]
    public class ModelComparaisonTests : HyperstoreTestBase
    {
        [TestMethod]
        public async Task CompareModels()
        {
            var store = await StoreBuilder.New().EnableScoping().Using<IIdGenerator>(r=>new Hyperstore.Modeling.Domain.LongIdGenerator()).CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            using (var session = store.BeginSession())
            {
                var lib = new Library(domain);
                lib.Name = "Lib1";
                for (int i = 0; i < 3; i++)
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

            var domain2 = await domain.CreateScopeAsync("domain2");
            Library lib2 = null;

            using (var session = store.BeginSession())
            {
                lib2 = domain2.GetEntities<Library>().First();

                // Remove 1 book
                var book = lib2.Books.First();
                lib2.Books.Remove(book);

                // Add 2 books
                var b = new Book(domain2);
                b.Title = "New book 1";
                b.Copies = 1;
                lib2.Books.Add(b);

                b = new Book(domain2);
                b.Title = "New book 2";
                b.Copies = 2;
                lib2.Books.Add(b);

                // Change one book property
                lib2.Books.First().Title = "Updated book";

                session.AcceptChanges();
            }

            Assert.AreEqual(2, domain2.GetDeletedElements().Count());
            Assert.AreEqual(4, lib2.Books.Count());
            Assert.AreEqual(6, domain2.GetScopeElements().Count()); // Books : 2 created, 1 updated, Library : 1 updated (Books property), LibraryHasBooks : 2 created
            Assert.AreEqual(5, domain2.GetUpdatedProperties().Count()); // New Books * 2 + 1 in the extendee domain
        }

    }
}
