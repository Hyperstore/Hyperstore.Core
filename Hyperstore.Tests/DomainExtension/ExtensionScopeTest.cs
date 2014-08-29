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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Hyperstore.Modeling;
using Hyperstore.Tests.Model;
using System.Linq;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Domain;

namespace Hyperstore.Tests.DomainExtension
{
    [TestClass]
    public class ExtensionScopeTest
    {
        [TestMethod]
        public async Task TestMethod1()
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

            Assert.AreEqual("xxxx", xLib.Name);
            Assert.AreEqual("My Library", lib.Name);

            Assert.AreEqual(12, xLib.Books.Count);
            Assert.AreEqual(10, lib.Books.Count);

            using (var session = store.BeginSession())
            {
                xLib.Books.Remove(extensionBook);
                session.AcceptChanges();
            }
            
            Assert.AreEqual(11, xLib.Books.Count);
            Assert.AreEqual(10, lib.Books.Count);

            using (var session = store.BeginSession())
            {
                extension.Commands.ProcessCommands(new RemoveEntityCommand( extensionBook));
                session.AcceptChanges();
            }

            store.DomainModels.Unload(extension);

            Assert.AreEqual("My Library", lib.Name);
            AssertHelper.ThrowsException<Exception>(() => { var x = xLib.Books.Count; });
            Assert.AreEqual(10, lib.Books.Count);
        }
    }
}
