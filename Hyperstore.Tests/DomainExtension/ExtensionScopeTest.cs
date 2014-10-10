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

namespace Hyperstore.Tests.DomainExtension
{
    
    public class ExtensionScopeTest
    {
        [Fact]
        public async Task TestScope()
        {
            var store = await StoreBuilder.New().EnableScoping().CreateAsync();

            await store.Schemas
                            .New<LibraryDefinition>()                            
                            .CreateAsync();

            var domain = await store.DomainModels
                                        .New()
                                        .UsingIdGenerator(r => new LongIdGenerator())
                                        .CreateAsync("lib");

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

            var extension = await domain.CreateScopeAsync("X1");
            var xLib = extension.GetEntities<Library>().First();

            Book extensionBook;
            using (var session = store.BeginSession())
            {
                xLib.Name = "xxxx";

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

            Assert.Equal("xxxx", xLib.Name);
            Assert.Equal("My Library", lib.Name);

            Assert.Equal(12, xLib.Books.Count);
            Assert.Equal(10, lib.Books.Count);

            using (var session = store.BeginSession())
            {
                xLib.Books.Remove(extensionBook);
                session.AcceptChanges();
            }
            
            Assert.Equal(11, xLib.Books.Count);
            Assert.Equal(10, lib.Books.Count);

            store.DomainModels.Unload(extension);

            Assert.Equal("My Library", lib.Name);
            Assert.Throws<UnloadedDomainException>(() => {
                var x = xLib.Books.Count; 
            });
            Assert.Equal(10, lib.Books.Count);
        }
    }
}
