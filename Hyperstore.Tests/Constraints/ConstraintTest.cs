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
    public class ConstraintsTest
    {
        private IHyperstore store;
        private IDomainModel domain;
        private ISchema schema;

        private async Task CreateDomain()
        {
            store = await StoreBuilder.New().EnableScoping().CreateAsync();

            schema = await store.Schemas
                            .New<LibraryDefinition>()
                            .CreateAsync();

            domain = await store.DomainModels
                                        .New()
                                        .UsingIdGenerator(r => new LongIdGenerator())
                                        .CreateAsync("lib");
        }

        [TestMethod]
        public async Task ConstraintNotExecuted()
        {
            await CreateDomain();

            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "My Library"; // OK
                lib.Email = "toto";      // Failed
                // rollback;
            }

            using (var session = store.BeginSession(new SessionConfiguration { Mode = SessionMode.SkipConstraints }))
            {
                lib = new Library(domain);
                lib.Name = "My Library"; // OK
                lib.Email = "toto";      // Failed
                session.AcceptChanges();
            }
        }


        [TestMethod]
        public async Task TestCheckConstraint()
        {
            await CreateDomain();

            var ex = AssertHelper.ThrowsException<Exception>(() =>
                {
                    Library lib;
                    using (var session = store.BeginSession())
                    {
                        lib = new Library(domain);
                        lib.Name = "My Library"; // OK
                        lib.Email = "toto";      // Failed
                        session.AcceptChanges();
                    }
                });

            Assert.AreEqual("[Error] - Invalid email address toto for My Library ", ex.Message);
        }

        [TestMethod]
        public async Task TestCheckConstraint2()
        {
            await CreateDomain();

            var ex = AssertHelper.ThrowsException<Exception>(() =>
            {
                Library lib;
                using (var session = store.BeginSession())
                {
                    lib = new Library(domain);
                    lib.Name = null; // Failed
                    lib.Email = "contact@zenasoft.com"; // OK
                    session.AcceptChanges();
                }
            });
            Assert.AreEqual("[Error] - Name is required for element  (lib:1). ", ex.Message);
        }

        [TestMethod]
        public async Task TestValidateConstraint()
        {
            await CreateDomain();
            LibraryDefinition.Library.AddConstraint<Library>(self => self.Name != "abcd").Message("error").Category("test").Create();

                Library lib;
                using (var session = store.BeginSession())
                {
                    lib = new Library(domain);
                    lib.Name = "abcd"; // Failed
                    lib.Email = "contact@zenasoft.com"; // OK
                    session.AcceptChanges();
                }

                var result = schema.Constraints.Validate(domain.GetElements());
                Assert.AreEqual(1, result.Messages.Count());
                Assert.AreEqual("error for element abcd (lib:1).", result.Messages.First().Message);

                result = schema.Constraints.Validate(domain.GetElements(), "test"); // With a valid category
                Assert.AreEqual(1, result.Messages.Count());
                Assert.AreEqual("error for element abcd (lib:1).", result.Messages.First().Message);


                result = schema.Constraints.Validate(domain.GetElements(), "test2"); // Unknow category
                Assert.AreEqual(0, result.Messages.Count());
        }
    }
}
