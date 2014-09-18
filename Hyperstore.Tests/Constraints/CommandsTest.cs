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
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Hyperstore.Modeling.Metadata.Constraints;

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
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

        [TestMethod]
        [TestCategory("Constraints")]
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
            Assert.IsTrue(result.Messages.Count() == 1);
        }

        [TestMethod]
        [TestCategory("Constraints")]
        public async Task ExplicitConstraint()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            TestDomainDefinition.XExtendsBaseClass.AddConstraint(self =>
                self.Name == "momo"
                , "Not null").Create();

            using (var s = store.BeginSession())
            {
                var a = new XExtendsBaseClass(domain);
                a.Name = "mama";
                s.AcceptChanges();
            } // Pas d'erreur

            var result = schema.Constraints.Validate(domain.GetElements());
            Assert.IsTrue(result.Messages.Count() == 1);

        }

        [TestMethod]
        [TestCategory("Constraints")]
        public async Task ImplicitConstraint()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            TestDomainDefinition.XExtendsBaseClass.AddImplicitConstraint(self =>
                self.Name == "momo"
                , "Not null").Create();

            try
            {
                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    a.Name = "mama";
                    s.AcceptChanges();
                }

                Assert.Inconclusive();
            }
            catch (SessionException ex)
            {
                Assert.IsTrue(ex.Messages.Count() == 1);
            }
        }

        [TestMethod]
        [TestCategory("Constraints")]
        public async Task Contraint_Error_Notification()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            TestDomainDefinition.XExtendsBaseClass.AddImplicitConstraint(self => self.Name != null, "Not null").Create();
            bool sawError = false;
            domain.Events.OnErrors.Subscribe(m => { sawError = true; });

            try
            {
                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    s.AcceptChanges();
                }

                Assert.Inconclusive();
            }
            catch (SessionException ex)
            {
                Assert.IsTrue(ex.Messages.Count() == 1);
            }
            Assert.AreEqual(true, sawError);
        }

        [TestMethod]
        [TestCategory("Constraints")]
        public async Task EatConstraintException()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            TestDomainDefinition.XExtendsBaseClass.AddImplicitConstraint(self => self.Name != null, "Not null").Create();
            domain.Events.OnErrors.Subscribe(m => { m.SetSilentMode(); });

            using (var s = domain.Store.BeginSession())
            {
                var a = new XExtendsBaseClass(domain);
                s.AcceptChanges();
            }
        }

        [TestMethod]
        [TestCategory("Constraints")]
        public async Task MultiConstraints()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
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
                TestDomainDefinition.XExtendsBaseClass.AddImplicitConstraint(self =>
                {
                    System.Threading.Interlocked.Decrement(ref cx);
                    return self.Value > x;
                }, "error").Create();
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

            Assert.AreEqual(0, cx);
        }

        [TestMethod]
        [TestCategory("Constraints")]
        public async Task Constraint_Cannot_modify_the_model()
        {
            await AssertHelper.ThrowsException<SessionException>(async () =>
            {
                Identity aid = null;
                var store = await StoreBuilder.New().CreateAsync();
                await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");

                TestDomainDefinition.XExtendsBaseClass.AddImplicitConstraint(self =>
                    {
                        self.Name = "xxx"; // Forbidden
                        return true;
                    }
                , "Not null").Create();

                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    aid = ((IModelElement)a).Id;
                    s.AcceptChanges();
                }
            });
        }

        [TestMethod]
        [TestCategory("Constraints")]
        public async Task Inherited_constraint()
        {
            await AssertHelper.ThrowsException<SessionException>(async () =>
            {
                var store = await StoreBuilder.New().CreateAsync();
                await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");

                TestDomainDefinition.XExtendsBaseClass.AddImplicitConstraint(self =>
                {
                    return self.Name != "xxx";
                }
                , "Not null").Create();

                using (var s = domain.Store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    a.Name = "xxx";
                    s.AcceptChanges();
                }
            });
        }

        [TestMethod]
        public async Task RelationshipConstraintTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            TestDomainDefinition.XReferencesY.AddImplicitConstraint(r => r.Weight > 0, "error").Create();

            AssertHelper.ThrowsException<SessionException>(
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
