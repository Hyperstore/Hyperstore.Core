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
 
using Xunit;
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
    
    public class MetaModelTest 
    {
        [Fact]
        
        public async Task PrimitivesIsATest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.DomainModels.New().CreateAsync("test");

            var metaClass = PrimitivesSchema.SchemaEntitySchema;
            Assert.True(metaClass.IsA(metaClass));

            var metaRel = PrimitivesSchema.SchemaRelationshipSchema;
            Assert.True(metaRel.IsA(metaRel));
            Assert.True(metaRel.IsA(metaClass));

            Assert.True(PrimitivesSchema.StringSchema.IsA(PrimitivesSchema.SchemaElementSchema));
        }

        [Fact]
        
        public async Task IsAMetaTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            var a = store.GetSchemaInfo<CultureInfo>();

            Assert.True(a.SchemaInfo.IsA(PrimitivesSchema.SchemaElementSchema));
        }

        [Fact]
        
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

            Assert.True(a.SchemaInfo.IsA(store.GetSchemaEntity<XExtendsBaseClass>()));
            Assert.True(a.SchemaInfo.IsA(PrimitivesSchema.ModelEntitySchema));
            Assert.True(a.SchemaInfo.SchemaInfo.IsA(PrimitivesSchema.SchemaEntitySchema));
            Assert.False(a.SchemaInfo.IsA(PrimitivesSchema.SchemaEntitySchema));
        }

        [Fact]
        
        public async Task IsAInheritTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            IModelElement a;
            using (var s = store.BeginSession())
            {
                a = new XExtendsBaseClass(dm);
                s.AcceptChanges();
            }

            Assert.True(a.SchemaInfo.IsA(schema.Definition.AbstractClass));

            Assert.True(a.SchemaInfo.IsA(PrimitivesSchema.ModelEntitySchema));
            Assert.False(((ISchemaElement)schema.Definition.AbstractClass).IsA(a.SchemaInfo));
        }

        [Fact]
        
        public async Task GetAllMetadatas()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            Assert.Equal(15, schema.GetSchemaInfos().Count());
            Assert.Equal(3, schema.GetSchemaEntities().Count());
            Assert.Equal(3, schema.GetSchemaRelationships().Count());
        }

        [Fact]
        
        public async Task GetMetaRelationshipsByTerminaison()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            Assert.Equal(1, schema.GetSchemaRelationships().Count(r => r.End.IsA(schema.Definition.XExtendsBaseClass)));
            Assert.Equal(2, schema.GetSchemaRelationships().Count(r => r.Start.IsA(schema.Definition.XExtendsBaseClass)));
        }

        [Fact]
        public async Task UnknowMetadata_raises_exception()
        {
            await Assert.ThrowsAsync<MetadataNotFoundException>( async () =>
            {
                var store = await StoreBuilder.New().CreateAsync();
                var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
                var domain = await store.DomainModels.New().CreateAsync("Test");
                var metadata = schema.GetSchemaInfo("Z");
            });
        }

        [Fact]
        public async Task DefineMetaclassTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            var metadata = store.GetSchemaInfo<XExtendsBaseClass>();
            Assert.NotNull(metadata);
        }

        [Fact]
        public async Task DefinePropertyMetadataTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            ISchemaEntity metadata = schema.Definition.XExtendsBaseClass;
            Assert.NotNull(metadata);
            Assert.Equal(2, metadata.GetProperties(true).Count());
            Assert.NotNull(metadata.GetProperty("Name"));
            Assert.Equal(1, metadata.GetProperties(false).Count());
        }

        enum ETest { A, B };

        [Fact]
        public async Task DefineEnumPropertyTest_Autodefinition()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test") as IUpdatableDomainModel;

            using( var s = store.BeginSession() )
            {
                ISchemaEntity metadata = schema.Definition.XExtendsBaseClass;
                var prop = metadata.DefineProperty<ETest>( "enum" );

                IModelElement a = new XExtendsBaseClass( domain );

                var pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.Equal(0, pv.CurrentVersion);
                Assert.Equal( ETest.A, pv.Value );
                
                domain.SetPropertyValue( a, prop, ETest.B );
                pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.NotEqual(0, pv.CurrentVersion);
                Assert.Equal(ETest.B, pv.Value);
                s.AcceptChanges();
            }
        }

        [Fact]
        public async Task DefineEnumPropertyTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test") as IUpdatableDomainModel;

            using (var s = store.BeginSession())
            {
                ISchemaElement metadata = schema.Definition.XExtendsBaseClass;
                var e = new Hyperstore.Modeling.Metadata.Primitives.EnumPrimitive(schema, typeof(ETest));
                var prop = metadata.DefineProperty<ETest>("enum");

                IModelElement a = new XExtendsBaseClass(domain);

                var pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.Equal(0, pv.CurrentVersion);
                Assert.Equal(ETest.A, pv.Value);

                domain.SetPropertyValue(a, prop, ETest.B);
                pv = domain.GetPropertyValue(a.Id, a.SchemaInfo, prop);
                Assert.NotEqual(0, pv.CurrentVersion);
                Assert.Equal(ETest.B, pv.Value);
                s.AcceptChanges();
            }
        }

    }
}
