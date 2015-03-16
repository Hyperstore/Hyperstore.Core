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
using Xunit;
using System.Threading.Tasks;
using Hyperstore.Modeling;
using Hyperstore.Tests.Model;
using System.Linq;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Domain;
using System.IO;
using System.Text;

namespace Hyperstore.Tests.DomainExtension
{
    
    public class ExtensionScopeTest
    {
        [Fact(DisplayName="TestScope")]
        public async Task TestScope()
        {
            // Enable scoping
            var store = await StoreBuilder.New().EnableScoping().CreateAsync();

            // Load library schema
            await store.Schemas
                            .New<LibraryDefinition>()                            
                            .CreateAsync();

            // Create a new domain
            var domain = await store.DomainModels
                                        .New()
                                        .UsingIdGenerator(r => new LongIdGenerator())
                                        .CreateAsync("lib");

            // Populate domain with a library and 10 books
            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "My Library";
                for (int i = 1; i <= 10;i++)
                {
                    var b = new Book(domain);
                    b.Title = "Book " + i.ToString();
                    b.Copies = i;
                    lib.Books.Add(b);
                }
                session.AcceptChanges();
            }

            // Load an extension
            var extension = await domain.CreateScopeAsync("X1");
            // Get a library reference within this extension
            var xLib = extension.GetEntities<Library>().First();

            // From here, changes are made in the extension (the extended domain is read-only)
            Book extensionBook;
            using (var session = store.BeginSession())
            {
                // Change some library properties
                xLib.Name = "xxxx";

                //Add two books
                extensionBook = new Book(extension);
                extensionBook.Copies = 100;
                extensionBook.Title = "extension book";
                xLib.Books.Add(extensionBook);
                var b = new Book(extension);
                b.Copies = 100;
                b.Title = "extension book 2";
                xLib.Books.Add(b);
                session.AcceptChanges();
            }

            // Check changes
            Assert.Equal("xxxx", xLib.Name); 
            Assert.Equal("My Library", lib.Name); // Initial property always the same

            Assert.Equal(12, xLib.Books.Count);
            Assert.Equal(10, lib.Books.Count);

            // Remove one book in the extension
            using (var session = store.BeginSession())
            {
                xLib.Books.Remove(extensionBook);
                session.AcceptChanges();
            }

            Assert.Equal(2, extension.GetDeletedElements().Count());    // extensionBook and one relationship LibraryHasBooks
            Assert.Equal(3, extension.GetUpdatedProperties().Count());  // Lib.Name, Book.Title, Book.Copies
            Assert.Equal(3, extension.GetScopeElements().Count());      // library, b (book) and one relationship LibraryHasBooks

            Assert.Equal(11, xLib.Books.Count);
            Assert.Equal(10, lib.Books.Count);

            // Can change a property in the extended domain
            lib.Name = lib.Name + "2";
            Assert.Equal("xxxx", xLib.Name);

            using (var session = store.BeginSession())
            {
                xLib.Books.Remove(xLib.Books.Last());
                session.AcceptChanges();
            }

            // Save only changes
            var text = Hyperstore.Modeling.Serialization.HyperstoreSerializer.SerializeScope(extension);

            // Unload extension
            store.DomainModels.Unload(extension);

            Assert.Equal("My Library2", lib.Name);
            Assert.Throws<UnloadedDomainException>(() => {
                var x = xLib.Books.Count; 
            });
            Assert.Equal(10, lib.Books.Count);

            // Reload extension
            extension = await domain.CreateScopeAsync("X1");
            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                Hyperstore.Modeling.Serialization.XmlDeserializer.Deserialize(reader, extension);
            }
            xLib = extension.GetEntities<Library>().First();
            Assert.Equal("xxxx", xLib.Name);
            Assert.Equal(10, xLib.Books.Count);
        }
    }
}
