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
    
    public class ConstraintsTest
    {
        private IHyperstore store;
        private IDomainModel domain;
        private ISchema<LibraryDefinition> schema;

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

        [Fact]
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


        [Fact]
        public async Task TestCheckConstraint()
        {
            await CreateDomain();

            var ex = Assert.Throws<SessionException>(() =>
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

            Assert.Equal("[Error] - Invalid email address toto for My Library ", ex.Message);
        }

        [Fact]
        public async Task TestCheckConstraint2()
        {
            await CreateDomain();

            var ex = Assert.Throws<SessionException>(() =>
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
            Assert.Equal("[Error] - Name is required for element  (lib:1). ", ex.Message);
        }

        [Fact]
        public async Task TestValidateConstraint()
        {
            await CreateDomain();
            schema.Definition.Library.AddConstraint<Library>(self => self.Name != "abcd").Message("error").Category("test").Register();

                Library lib;
                using (var session = store.BeginSession())
                {
                    lib = new Library(domain);
                    lib.Name = "abcd"; // Failed
                    lib.Email = "contact@zenasoft.com"; // OK
                    session.AcceptChanges();
                }

                var result = schema.Constraints.Validate(domain.GetElements());
                Assert.Equal(1, result.Messages.Count());
                Assert.Equal("error for element abcd (lib:1).", result.Messages.First().Message);

                result = schema.Constraints.Validate(domain.GetElements(), "test"); // With a valid category
                Assert.Equal(1, result.Messages.Count());
                Assert.Equal("error for element abcd (lib:1).", result.Messages.First().Message);


                result = schema.Constraints.Validate(domain.GetElements(), "test2"); // Unknow category
                Assert.Equal(0, result.Messages.Count());
        }
    }
}
