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
using System.Threading.Tasks;
using Hyperstore.Modeling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hyperstore.Modeling.Commands;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Extension
{
    [TestClass]
    public class ExtensionsTest : HyperstoreTestBase
    {
        class CategoryEx : Category
        {
            protected CategoryEx() { }

            public CategoryEx(IDomainModel domainModel)
                : base(domainModel)
            {
            }

            public int XValue
            {
                get { return GetPropertyValue<int>("XValue"); }
                set { SetPropertyValue("XValue", value); }
            }
        }

        class ExtensionsDomainDefinition : SchemaDefinition
        {
            public ExtensionsDomainDefinition()
                : base("Hyperstore.Tests")
            {
                Using<Hyperstore.Modeling.HyperGraph.IIdGenerator>(r => new Hyperstore.Modeling.Domain.LongIdGenerator());
            }

            protected override void DefineSchema(ISchema domainModel)
            {
                ISchemaEntity categoryEx = new Hyperstore.Modeling.Metadata.SchemaEntity<CategoryEx>(domainModel, domainModel.Store.GetSchemaEntity<Category>());
                categoryEx.DefineProperty<int>("XValue");
            }
        }

        class Category : ModelEntity
        {
            protected Category() { }

            public Category(IDomainModel domainModel, Identity id=null)
                : base(domainModel, id:id)
            {
            }

            public int Value
            {
                get { return GetPropertyValue<int>("Value"); }
                set { SetPropertyValue("Value", value); }
            }

            public string Name
            {
                get { return GetPropertyValue<string>("Name"); }
                set { SetPropertyValue("Name", value); }
            }
        }

        class InitialDomainDefinition : SchemaDefinition
        {
            public InitialDomainDefinition()
                : base("Hyperstore.Tests")
            {
                Using<Hyperstore.Modeling.HyperGraph.IIdGenerator>(r => new Hyperstore.Modeling.Domain.LongIdGenerator());
            }
            protected override void DefineSchema(ISchema domainModel)
            {
                ISchemaEntity category = new Hyperstore.Modeling.Metadata.SchemaEntity<Category>(domainModel);
                category.DefineProperty<string>("Name");
                category.DefineProperty<int>("Value");
            }
        }

        [TestMethod]
        public async Task ExtensionGetExisting()
        {
            var store = await StoreBuilder.New().EnableScoping().CreateAsync();
            var schema = await store.Schemas.New<InitialDomainDefinition>().CreateAsync();
            var initial = await store.DomainModels.New().CreateAsync("D1");

            Category cat;
            using (var s = store.BeginSession())
            {
                cat = new Category(initial);
                cat.Value = 10;
                s.AcceptChanges();
            }

            await schema.LoadSchemaExtension(new ExtensionsDomainDefinition());
            var extended = await initial.CreateScopeAsync("Ex1");

            Assert.IsNotNull(store.GetElement<CategoryEx>(((IModelElement)cat).Id));
        }

        //[TestMethod]
        //public async Task Extension_constraint_in_updatable_mode()
        //{
        //    // En mode updatable, les contraintes du domaine étendu s'appliquent
        //    await AssertHelper.ThrowsException<SessionException>(async () =>
        //        {
        //            var store = await StoreBuilder.Init().EnableExtensions().CreateStore();
        //            var schema = await store.Schemas.New<InitialDomainDefinition>().CreateAsync();
        //            var initial = await store.DomainModels.New().CreateAsync("D1");

        //            store.GetSchemaEntity<Category>().AddImplicitConstraint<Category>(c => c.Value < 10, "Invalid value");

        //            Category cat;
        //            using (var s = store.BeginSession())
        //            {
        //                cat = new Category(initial);
        //                cat.Value = 1;
        //                s.AcceptChanges();
        //            }

        //            await schema.LoadSchemaExtension(new ExtensionsDomainDefinition());
        //            var extended = await initial.LoadExtensionAsync("Ex1", ExtendedMode.Updatable);

        //            CategoryEx catx;
        //            using (var s = store.BeginSession())
        //            {
        //                catx = store.GetElement<CategoryEx>(((IModelElement)cat).Id);
        //                catx.Value = 10; // Doit planter
        //                s.AcceptChanges();
        //            }
        //        });
        //}

        //[TestMethod]
        //public async Task Extension_constraint()
        //{
        //    await AssertHelper.ThrowsException<SessionException>(async () =>
        //        {
        //            var store = await StoreBuilder.New().EnableScoping().CreateAsync();
        //            var schema = await store.Schemas.New<InitialDomainDefinition>().CreateAsync();
        //            var initial = await store.DomainModels.New().CreateAsync("D1");
        //            await schema.LoadSchemaExtension( new ExtensionsDomainDefinition());
        //            var extended = await initial.CreateScopeAsync("Ex1");

        //            store.GetSchemaEntity<Category>().AddImplicitConstraint<Category>(c => c.Value < 10, "Invalid value");

        //            Category cat;
        //            using (var s = store.BeginSession())
        //            {
        //                cat = new Category(initial);
        //                cat.Value = 10;
        //                s.AcceptChanges();
        //            }
        //        });
        //}

        [TestMethod]
        public async Task Extension_constraint_in_readonly_mode()
        {
            var store = await StoreBuilder.New().EnableScoping().CreateAsync();
            var schema = await store.Schemas.New<InitialDomainDefinition>().CreateAsync();
            var initial = await store.DomainModels.New().CreateAsync("D1");

            store.GetSchemaEntity<Category>().AddImplicitConstraint<Category>(c => c.Value < 10, "Invalid value");

            Category cat;
            using (var s = store.BeginSession())
            {
                cat = new Category(initial);
                cat.Value = 1;
                s.AcceptChanges();
            }

            await schema.LoadSchemaExtension( new ExtensionsDomainDefinition(), SchemaConstraintExtensionMode.Replace);
            var extended = await initial.CreateScopeAsync("Ex1");

            CategoryEx catx;
            using (var s = store.BeginSession())
            {
                catx = store.GetElement<CategoryEx>(((IModelElement)cat).Id);
                catx.Value = 10; // Pas d'erreur car la contrainte ne s'applique pas
                s.AcceptChanges();
            }
        }

        //[TestMethod]
        //public async Task TestExtension()
        //{
        //    var initialResult = @"<?xml version=""1.0"" encoding=""utf-8""?><domain name=""d1""><model><elements><element id=""d1:1"" metadata=""hyperstore.tests:extension.extensionstest+categoryex""><attributes><attribute name=""Value"">10</attribute><attribute name=""Name"">Cat x</attribute></attributes></element></elements><relationships /></model></domain>";
        //    var extensionResult = @"<?xml version=""1.0"" encoding=""utf-8""?><domain name=""d1""><model><elements><element id=""d1:1"" metadata=""hyperstore.tests:extension.extensionstest+categoryex""><attributes><attribute name=""XValue"">20</attribute></attributes></element></elements><relationships /></model></domain>";

        //    var store = await StoreBuilder.Init().EnableExtensions().CreateStore();
        //    var schema = await store.Schemas.New<InitialDomainDefinition>().CreateAsync();
        //    var initial = await store.DomainModels.New().CreateAsync("D1", new DomainConfiguration().UsesIdGenerator(r => new Hyperstore.Modeling.Domain.LongIdGenerator()));
        //    await schema.LoadSchemaExtension(new ExtensionsDomainDefinition());
        //    var extended = await initial.LoadExtensionAsync("Ex1", ExtendedMode.Updatable, new DomainConfiguration().UsesIdGenerator(r => new Hyperstore.Modeling.Domain.LongIdGenerator()));

        //    CategoryEx catx;
        //    using (var s = store.BeginSession())
        //    {
        //        catx = new CategoryEx(extended);
        //        catx.Name = "Cat x";
        //        catx.Value = 10;
        //        catx.XValue = 20;
        //        s.AcceptChanges();
        //    }

        //    using (var ms = new MemoryStream())
        //    {
        //        var ser = new Hyperstore.Modeling.Serialization.XmlDomainModelSerializer();
        //        await ser.Serialize(initial, ms);

        //        var result = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
        //        Assert.IsTrue(String.Compare(initialResult, result) == 0);
        //    }

        //    using (var ms = new MemoryStream())
        //    {
        //        var ser = new Hyperstore.Modeling.Serialization.XmlDomainModelSerializer();
        //        await ser.Serialize(extended, ms);
        //        var result = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
        //        Assert.IsTrue(String.Compare(extensionResult, result) == 0);
        //    }
        //}

        //[TestMethod]
        //public void ReadOnlyException()
        //{
        //    AssertHelper.ThrowsException<SessionException>(() =>
        //        {
        //            var store = new Hyperstore.Modeling.Store();
        //            var initial = store.LoadDomainModel("d1", new InitialDomainDefinition());
        //            var extended = store.LoadDomainModel("d1", new ExtensionsDomainDefinition(initial, ExtensionMode.ReadOnly));

        //            CategoryEx catx;
        //            using (var s = store.BeginSession())
        //            {
        //                catx = new CategoryEx(extended);
        //                s.AcceptChanges();
        //            }
        //        });
        //}    

        [TestMethod]
        public async Task ExtendedUnloadTest()
        {
            var store = await StoreBuilder.New().EnableScoping().CreateAsync();
            var initialSchema = await store.Schemas.New<InitialDomainDefinition>().CreateAsync();
            var initial = await store.DomainModels.New().CreateAsync("D1");

            // Create a category in the initial domain
            Category a = null;
            try
            {
                using (var tx = store.BeginSession())
                {
                    a = new Category(initial);
                    a.Name = "Classe A";
                    a.Value = 1;
                    tx.AcceptChanges();
                }
            }
            catch (SessionException)
            {
                Assert.Inconclusive();
            }

            // Add a constraint
            store.GetSchemaEntity<Category>().AddImplicitConstraint<Category>(
                ca =>
                    ca.Value > 0,
                "Value ==0");

            Random rnd = new Random(DateTime.Now.Millisecond);
            System.Threading.CancellationTokenSource cancel = new System.Threading.CancellationTokenSource();

            // Run 2 thread in parallel
            Task.Factory.StartNew(() =>
            {
                while (!cancel.Token.IsCancellationRequested)
                {
                    using (var s = store.BeginSession(new SessionConfiguration { Readonly = true, IsolationLevel = SessionIsolationLevel.Serializable }))
                    {
                        //  s.AcquireLock(LockType.Shared, a.Id.CreateAttributeIdentity("Value"));
                        var x = store.GetElement<Category>(((IModelElement)a).Id);
                        var v = x.Value;
                        Assert.AreEqual(false, v != 1 && v != 9);
                    }

                    Sleep(11);
                }
            }, cancel.Token);

            Task.Factory.StartNew(() =>
            {
                while (!cancel.Token.IsCancellationRequested)
                {
                    using (var s = store.BeginSession(new SessionConfiguration { Readonly = true, IsolationLevel = SessionIsolationLevel.Serializable }))
                    {
                        // s.AcquireLock(LockType.Shared, a.Id.CreateAttributeIdentity("Value"));
                        var x = store.GetElement<Category>(((IModelElement)a).Id);
                        var v = x.Value;
                        Assert.AreEqual(false, v != 1 && v != 9);
                    }
                    Sleep(7);
                }
            }, cancel.Token);

            // Load a schema extension
            await initialSchema.LoadSchemaExtension( new ExtensionsDomainDefinition());

            // Iterate to make hot load and unload of the extension
            for (int i = 1; i < 30; i++)
            {
                Sleep(100);

                var xDomain = await initial.CreateScopeAsync("Ex1");

                store.GetSchemaEntity<CategoryEx>().AddImplicitConstraint<CategoryEx>(ca => ca.Value < 10, "Value == 10");
                using (var tx = store.BeginSession())
                {
                    //tx.AcquireLock(LockType.Exclusive, a.Id.CreateAttributeIdentity("Value"));
                    var c = store.GetElement<CategoryEx>(((IModelElement)a).Id);
                    //  c.Text2 = "Classe C";
                    c.XValue = 2;
                    c.Value = 9;
                    tx.AcceptChanges();
                }

                var xx = store.GetElement<Category>(((IModelElement)a).Id).Value;
                Assert.AreEqual(9, xx);

                Sleep(100);
                store.DomainModels.Unload(xDomain);
            }

            cancel.Cancel(false);
        }


        [TestMethod]
        public async Task ExtendedDeleteElementTest()
        {
            var store = await StoreBuilder.New().EnableScoping().CreateAsync();
            var initialSchema = await store.Schemas.New<InitialDomainDefinition>().CreateAsync();
            var initial = await store.DomainModels.New().CreateAsync("D1");

            // Create a category in the initial domain
            Category a = null;
            try
            {
                using (var tx = store.BeginSession())
                {
                    a = new Category(initial);
                    a.Name = "Classe A";
                    a.Value = 1;

                    tx.AcceptChanges();
                }
            }
            catch (SessionException)
            {
                Assert.Inconclusive();
            }

            var xDomain = await initial.CreateScopeAsync("Ex1");
            var id = ((IModelElement)a).Id;
            using (var tx = store.BeginSession())
            {
                var c = store.GetElement<Category>(id);
                xDomain.Commands.ProcessCommands(new[] { new RemoveEntityCommand(c) });
                // rollback
            }

            Assert.IsNotNull(store.GetElement<Category>(id));

            using (var tx = store.BeginSession())
            {
                var c = store.GetElement<Category>(id);
                xDomain.Commands.ProcessCommands(new[] { new RemoveEntityCommand(c) });
                tx.AcceptChanges();
            }

            Assert.IsNull( store.GetElement<Category>(id));
            Assert.IsNull(xDomain.GetElement<Category>(id));
            Assert.IsNotNull(initial.GetElement<Category>(id));
            Assert.AreEqual(1, initial.GetElement<Category>(id).Value);

            using (var tx = store.BeginSession())
            {
                a = new Category(xDomain, id);
                a.Name = "Classe A";
                a.Value = 10;
                tx.AcceptChanges();
            }

            Assert.IsNotNull(store.GetElement<Category>(id));
            Assert.AreEqual(10, store.GetElement<Category>(id).Value);

            Assert.AreEqual(1, initial.GetElement<Category>(id).Value);

            
            store.Dispose();
        }
    }
}
