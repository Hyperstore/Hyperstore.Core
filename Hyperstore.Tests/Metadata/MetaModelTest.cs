//	Copyright ? 2013 - 2014, Alain Metge. All rights reserved.
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
        public async Task PrimitivesIsATest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.DomainModels.New().CreateAsync("test");

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
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            var a = store.GetSchemaInfo<CultureInfo>();

            Assert.IsTrue(a.SchemaInfo.IsA(PrimitivesSchema.SchemaElementSchema));
        }

        [TestMethod]
        [TestCategory("Meta")]
        public async Task IsATest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

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
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

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
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            Assert.AreEqual(15, schema.GetSchemaInfos().Count());
            Assert.AreEqual(3, schema.GetSchemaEntities().Count());
            Assert.AreEqual(3, schema.GetSchemaRelationships().Count());
        }

        [TestMethod]
        [TestCategory("Meta")]
        public async Task GetMetaRelationshipsByTerminaison()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            Assert.AreEqual(1, schema.GetSchemaRelationships().Count(r => r.End.IsA(TestDomainDefinition.XExtendsBaseClass)));
            Assert.AreEqual(2, schema.GetSchemaRelationships().Count(r => r.Start.IsA(TestDomainDefinition.XExtendsBaseClass)));
        }

        [TestMethod()]
        public async Task UnknowMetadata_raises_exception()
        {
            await AssertHelper.ThrowsException<MetadataNotFoundException>( async () =>
            {
                var store = await StoreBuilder.New().CreateAsync();
                var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");
                var metadata = schema.GetSchemaInfo("Z");
            });
        }

        [TestMethod()]
        public async Task DefineMetaclassTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            var metadata = store.GetSchemaInfo<XExtendsBaseClass>();
            Assert.IsNotNull(metadata);
        }

        [TestMethod()]
        public async Task DefinePropertyMetadataTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

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
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test") as IUpdatableDomainModel;

            using( var s = store.BeginSession() )
            {
                ISchemaEntity metadata = TestDomainDefinition.XExtendsBaseClass;
                var prop = metadata.DefineProperty<ETest>( "enum" );

                IModelElement a = new XExtendsBaseClass( domain );

                var pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.AreEqual(0, pv.CurrentVersion);
                Assert.AreEqual( ETest.A, pv.Value );
                
                domain.SetPropertyValue( a, prop, ETest.B );
                pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.AreNotEqual(0, pv.CurrentVersion);
                Assert.AreEqual(ETest.B, pv.Value);
                s.AcceptChanges();
            }
        }

        [TestMethod()]
        public async Task DefineEnumPropertyTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test") as IUpdatableDomainModel;

            using (var s = store.BeginSession())
            {
                ISchemaElement metadata = TestDomainDefinition.XExtendsBaseClass;
                var e = new Hyperstore.Modeling.Metadata.Primitives.EnumPrimitive(schema, typeof(ETest));
                var prop = metadata.DefineProperty<ETest>("enum");

                IModelElement a = new XExtendsBaseClass(domain);

                var pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.AreEqual(0, pv.CurrentVersion);
                Assert.AreEqual(ETest.A, pv.Value);

                domain.SetPropertyValue(a, prop, ETest.B);
                pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.AreNotEqual(0, pv.CurrentVersion);
                Assert.AreEqual(ETest.B, pv.Value);
                s.AcceptChanges();
            }
        }

    }
}
