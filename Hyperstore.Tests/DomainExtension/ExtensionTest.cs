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
using System.Threading.Tasks;
using Hyperstore.Modeling;
using Xunit;
using Hyperstore.Modeling.Commands;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Extension
{
    
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

        [Fact]
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

            Assert.NotNull(store.GetElement<CategoryEx>(((IModelElement)cat).Id));
        }

        //[Fact]
        //public async Task Extension_constraint_in_updatable_mode()
        //{
        //    // En mode updatable, les contraintes du domaine étendu s'appliquent
        //    await Assert.ThrowsAsync<SessionException>(async () =>
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

        //[Fact]
        //public async Task Extension_constraint()
        //{
        //    await Assert.ThrowsAsync<SessionException>(async () =>
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

        [Fact]
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

        //[Fact]
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
        //        Assert.True(String.Compare(initialResult, result) == 0);
        //    }

        //    using (var ms = new MemoryStream())
        //    {
        //        var ser = new Hyperstore.Modeling.Serialization.XmlDomainModelSerializer();
        //        await ser.Serialize(extended, ms);
        //        var result = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
        //        Assert.True(String.Compare(extensionResult, result) == 0);
        //    }
        //}

        //[Fact]
        //public void ReadOnlyException()
        //{
        //    Assert.ThrowsAsync<SessionException>(() =>
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

        [Fact]
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
                                throw new Exception("Inconclusive");
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
                        Assert.Equal(false, v != 1 && v != 9);
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
                        Assert.Equal(false, v != 1 && v != 9);
                    }
                    Sleep(7);
                }
            }, cancel.Token);

            // Load a schema extension
            await initialSchema.LoadSchemaExtension( new ExtensionsDomainDefinition());

            // Iterate to make hot load and unload of the extension
            for (int i = 1; i < 300; i++)
            {
                Sleep(10);

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
                Assert.Equal(9, xx);

                Sleep(12);
                store.DomainModels.Unload(xDomain);
            }

            cancel.Cancel(false);
        }


        [Fact]
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
                                throw new Exception("Inconclusive");
            }

            var xDomain = await initial.CreateScopeAsync("Ex1");
            var id = ((IModelElement)a).Id;
            using (var tx = store.BeginSession())
            {
                var c = store.GetElement<Category>(id);
                xDomain.Commands.ProcessCommands(new[] { new RemoveEntityCommand(c) });
                // rollback
            }

            Assert.NotNull(store.GetElement<Category>(id));

            using (var tx = store.BeginSession())
            {
                var c = store.GetElement<Category>(id);
                xDomain.Commands.ProcessCommands(new[] { new RemoveEntityCommand(c) });
                tx.AcceptChanges();
            }

            Assert.Null( store.GetElement<Category>(id));
            Assert.Null(xDomain.GetElement<Category>(id));
            Assert.NotNull(initial.GetElement<Category>(id));
            Assert.Equal(1, initial.GetElement<Category>(id).Value);

            using (var tx = store.BeginSession())
            {
                a = new Category(xDomain, id);
                a.Name = "Classe A";
                a.Value = 10;
                tx.AcceptChanges();
            }

            Assert.NotNull(store.GetElement<Category>(id));
            Assert.Equal(10, store.GetElement<Category>(id).Value);

            Assert.Equal(1, initial.GetElement<Category>(id).Value);

            
            store.Dispose();
        }
    }
}
