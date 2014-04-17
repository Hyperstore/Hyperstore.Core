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
 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Hyperstore.Modeling;
using System.Diagnostics;
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Metadata;
using System.Globalization;
using Hyperstore.Tests.Model;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.MemoryStore
{  
    [TestClass]
    public class MetaModelTest 
    {
        [TestMethod]
        [TestCategory("Meta")]
        public void PrimitivesIsATest()
        {
            var store = new Store();

            var metaClass = PrimitivesSchema.SchemaEntitySchema;
            Assert.IsTrue(metaClass.IsA(metaClass));

            var metaRel = PrimitivesSchema.SchemaRelationshipSchema;
            Assert.IsTrue(metaRel.IsA(metaRel));
            Assert.IsTrue(metaRel.IsA(metaClass));

            Assert.IsTrue(PrimitivesSchema.StringSchema.IsA(PrimitivesSchema.SchemaElementSchema));
        }

        [TestMethod]
        [TestCategory("Meta")]
        public async Task IsAMetaTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var dm = await store.CreateDomainModelAsync("Test");

            var a = store.GetSchemaInfo<CultureInfo>();

            Assert.IsTrue(a.SchemaInfo.IsA(PrimitivesSchema.SchemaElementSchema));
        }

        [TestMethod]
        [TestCategory("Meta")]
        public async Task IsATest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var dm = await store.CreateDomainModelAsync("Test");

            IModelElement a;
            using (var s = store.BeginSession())
            {
                a = new XExtendsBaseClass(dm);
                s.AcceptChanges();
            }

            Assert.IsTrue(a.SchemaInfo.IsA(store.GetSchemaEntity<XExtendsBaseClass>()));
            Assert.IsTrue(a.SchemaInfo.IsA(PrimitivesSchema.ModelEntitySchema));
            Assert.IsTrue(a.SchemaInfo.SchemaInfo.IsA(PrimitivesSchema.SchemaEntitySchema));
            Assert.IsFalse(a.SchemaInfo.IsA(PrimitivesSchema.SchemaEntitySchema));
        }

        [TestMethod]
        [TestCategory("Meta")]
        public async Task IsAInheritTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var dm = await store.CreateDomainModelAsync("Test");

            IModelElement a;
            using (var s = store.BeginSession())
            {
                a = new XExtendsBaseClass(dm);
                s.AcceptChanges();
            }

            Assert.IsTrue(a.SchemaInfo.IsA(TestDomainDefinition.AbstractClass));

            Assert.IsTrue(a.SchemaInfo.IsA(PrimitivesSchema.ModelEntitySchema));
            Assert.IsFalse(((ISchemaElement)TestDomainDefinition.AbstractClass).IsA(a.SchemaInfo));
        }

        [TestMethod]
        [TestCategory("Meta")]
        public async Task GetAllMetadatas()
        {
            var store = new Store();
            var schema = await store.LoadSchemaAsync(new TestDomainDefinition());
            var dm = await store.CreateDomainModelAsync("Test");

            Assert.AreEqual(15, schema.GetSchemaInfos().Count());
            Assert.AreEqual(3, schema.GetSchemaEntities().Count());
            Assert.AreEqual(3, schema.GetSchemaRelationships().Count());
        }

        [TestMethod]
        [TestCategory("Meta")]
        public async Task GetMetaRelationshipsByTerminaison()
        {
            var store = new Store();
            var schema = await store.LoadSchemaAsync(new TestDomainDefinition());
            var dm = await store.CreateDomainModelAsync("Test");

            Assert.AreEqual(1, schema.GetSchemaRelationships().Count(r => r.End.IsA(TestDomainDefinition.XExtendsBaseClass)));
            Assert.AreEqual(2, schema.GetSchemaRelationships().Count(r => r.Start.IsA(TestDomainDefinition.XExtendsBaseClass)));
        }

        [TestMethod()]
        public async void UnknowMetadata_raises_exception()
        {
            await AssertHelper.ThrowsException<MetadataNotFoundException>( async () =>
            {
                var store = new Store();
                var schema = await store.LoadSchemaAsync(new TestDomainDefinition());
                var domain = await store.CreateDomainModelAsync("Test");
                var metadata = schema.GetSchemaInfo("Z");
            });
        }

        [TestMethod()]
        public async Task DefineMetaclassTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var dm = await store.CreateDomainModelAsync("Test");
            var metadata = store.GetSchemaInfo<XExtendsBaseClass>();
            Assert.IsNotNull(metadata);
        }

        [TestMethod()]
        public async Task DefinePropertyMetadataTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            ISchemaEntity metadata = TestDomainDefinition.XExtendsBaseClass;
            Assert.IsNotNull(metadata);
            Assert.AreEqual(2, metadata.GetProperties(true).Count());
            Assert.IsNotNull(metadata.GetProperty("Name"));
            Assert.AreEqual(1, metadata.GetProperties(false).Count());
        }

        enum ETest { A, B };

        [TestMethod()]
        public async Task DefineEnumPropertyTest_Autodefinition()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test") as IUpdatableDomainModel;

            using( var s = store.BeginSession() )
            {
                ISchemaEntity metadata = TestDomainDefinition.XExtendsBaseClass;
                var prop = metadata.DefineProperty<ETest>( "enum" );

                var a = new XExtendsBaseClass( domain );

                var pv = domain.GetPropertyValue(((IModelElement)a).Id, TestDomainDefinition.XExtendsBaseClass, prop);
                Assert.AreEqual(0, pv.CurrentVersion);
                Assert.AreEqual( ETest.A, pv.Value );
                
                domain.SetPropertyValue( a, prop, ETest.B );
                pv = domain.GetPropertyValue(((IModelElement)a).Id, TestDomainDefinition.XExtendsBaseClass, prop);
                Assert.AreEqual(1, pv.CurrentVersion);
                Assert.AreEqual(ETest.B, pv.Value);
                s.AcceptChanges();
            }
        }

        [TestMethod()]
        public async Task DefineEnumPropertyTest()
        {
            var store = new Store();
            var schema = await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test") as IUpdatableDomainModel;

            using (var s = store.BeginSession())
            {
                ISchemaElement metadata = TestDomainDefinition.XExtendsBaseClass;
                var e = new Hyperstore.Modeling.Metadata.Primitives.EnumPrimitive(schema, typeof(ETest));
                var prop = metadata.DefineProperty<ETest>("enum");

                var a = new XExtendsBaseClass(domain);

                var pv = domain.GetPropertyValue(((IModelElement)a).Id, TestDomainDefinition.XExtendsBaseClass, prop);
                Assert.AreEqual(0, pv.CurrentVersion);
                Assert.AreEqual(ETest.A, pv.Value);

                domain.SetPropertyValue(a, prop, ETest.B);
                pv = domain.GetPropertyValue(((IModelElement)a).Id, TestDomainDefinition.XExtendsBaseClass, prop);
                Assert.AreEqual(1, pv.CurrentVersion);
                Assert.AreEqual(ETest.B, pv.Value);
                s.AcceptChanges();
            }
        }

    }
}
