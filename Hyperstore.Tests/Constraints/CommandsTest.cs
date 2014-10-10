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
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Tests.Model;
using Xunit;
using System.Threading.Tasks;
using Hyperstore.Modeling.Metadata.Constraints;

namespace Hyperstore.Tests.Commands
{
    
    public class ConstraintsTest
    {
        [Constraint]
        class Constraint : IValidationConstraint<XExtendsBaseClass>
        {
            public string Category
            {
                get { return null; }
            }

            public void ExecuteConstraint(XExtendsBaseClass self, ConstraintContext ctx)
            {
                if (!(self.Name == "momo"))
                {
                    ctx.CreateErrorMessage("Invalid value", "Name");
                }
            }
        }

        [Fact]
        
        public async Task ConstraintByComposition()
        {
            var store = await StoreBuilder.New().ComposeWith(typeof(ConstraintsTest).Assembly).CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            using (var s = store.BeginSession())
            {
                var a = new XExtendsBaseClass(domain);
                a.Name = "mama";
                s.AcceptChanges();
            } // Pas d'erreur


            var result = schema.Constraints.Validate(domain.GetElements());
            Assert.True(result.Messages.Count() == 1);
        }

        [Fact]
        
        public async Task ExplicitConstraint()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            schema.Definition.XExtendsBaseClass.AddConstraint(self =>
                self.Name == "momo"
                , "Not null").Register();

            using (var s = store.BeginSession())
            {
                var a = new XExtendsBaseClass(domain);
                a.Name = "mama";
                s.AcceptChanges();
            } // Pas d'erreur

            var result = schema.Constraints.Validate(domain.GetElements());
            Assert.True(result.Messages.Count() == 1);

        }

        [Fact]
        
        public async Task ImplicitConstraint()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            schema.Definition.XExtendsBaseClass.AddImplicitConstraint(self =>
                self.Name == "momo"
                , "Not null").Register();

            try
            {
                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    a.Name = "mama";
                    s.AcceptChanges();
                }

                                throw new Exception("Inconclusive");
            }
            catch (SessionException ex)
            {
                Assert.True(ex.Messages.Count() == 1);
            }
        }

        [Fact]
        
        public async Task Contraint_Error_Notification()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            schema.Definition.XExtendsBaseClass.AddImplicitConstraint(self => self.Name != null, "Not null").Register();
            bool sawError = false;
            domain.Events.OnErrors.Subscribe(m => { sawError = true; });

            try
            {
                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    s.AcceptChanges();
                }

                                throw new Exception("Inconclusive");
            }
            catch (SessionException ex)
            {
                Assert.True(ex.Messages.Count() == 1);
            }
            Assert.Equal(true, sawError);
        }

        [Fact]
        
        public async Task EatConstraintException()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            schema.Definition.XExtendsBaseClass.AddImplicitConstraint(self => self.Name != null, "Not null").Register();
            domain.Events.OnErrors.Subscribe(m => { m.SetSilentMode(); });

            using (var s = domain.Store.BeginSession())
            {
                var a = new XExtendsBaseClass(domain);
                s.AcceptChanges();
            }
        }

       // [Fact]
        
        public async Task MultiConstraints()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            // Loading domain
            using (var session = store.BeginSession(new SessionConfiguration { Mode = SessionMode.Loading | SessionMode.SkipConstraints }))
            {
                /// loading data
                session.AcceptChanges();
            }

            var max = 200;
            var nbElem = 1000;
            int cx = max * nbElem;
            for (int i = 0; i < max; i++)
            {
                var x = i;
                schema.Definition.XExtendsBaseClass.AddImplicitConstraint(self =>
                {
                    System.Threading.Interlocked.Decrement(ref cx);
                    return self.Value > x;
                }, "error").Register();
            }

            using (var s = domain.Store.BeginSession())
            {
                for (int j = 0; j < nbElem; j++)
                {
                    var a = new XExtendsBaseClass(domain);
                    a.Value = max + 1;
                }
                s.AcceptChanges();
            }

            Assert.Equal(0, cx);
        }

        [Fact]
        
        public async Task Constraint_Cannot_modify_the_model()
        {
            await Assert.ThrowsAsync<SessionException>(async () =>
            {
                Identity aid = null;
                var store = await StoreBuilder.New().CreateAsync();
                var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");

                schema.Definition.XExtendsBaseClass.AddImplicitConstraint(self =>
                    {
                        self.Name = "xxx"; // Forbidden
                        return true;
                    }
                , "Not null").Register();

                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    aid = ((IModelElement)a).Id;
                    s.AcceptChanges();
                }
            });
        }

        [Fact]
        
        public async Task Inherited_constraint()
        {
            await Assert.ThrowsAsync<SessionException>(async () =>
            {
                var store = await StoreBuilder.New().CreateAsync();
                var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");

                schema.Definition.XExtendsBaseClass.AddImplicitConstraint(self =>
                {
                    return self.Name != "xxx";
                }
                , "Not null").Register();

                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    a.Name = "xxx";
                    s.AcceptChanges();
                }
            });
        }

        [Fact]
        public async Task RelationshipConstraintTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            schema.Definition.XReferencesY.AddImplicitConstraint(r => r.Weight > 0, "error").Register();

            Assert.Throws<SessionException>(
                () =>
                {
                    XReferencesY rel = null;
                    using (var s = store.BeginSession())
                    {
                        var start = new XExtendsBaseClass(domain);
                        var end = new YClass(domain);
                        rel = new XReferencesY(start, end);
                        rel.YRelation = end;
                        s.AcceptChanges();
                    }
                });
        }
    }
}
