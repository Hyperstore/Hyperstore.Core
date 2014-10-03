using Hyperstore.Modeling;
//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
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
// limitations under the License.using Hyperstore.Modeling;
using Hyperstore.Modeling.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hyperstore.XTests
{
    public class SchemaTests
    {
        [Fact]
        public async Task CheckPrimitives()
        {
            var domain = await StoreBuilder.CreateDomain<MySchemaDefinition>("Test");
            var store = domain.Store;
            Assert.NotNull(store.GetSchemaInfo<string>());
            Assert.NotNull(store.GetSchemaInfo<int>());
            Assert.NotNull(store.GetSchemaInfo<double>());
        }

        [Fact]
        public async Task SchemaEvents()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var def = new MySchemaDefinition();
            var schema = await store.Schemas.New(def).CreateAsync();
            Assert.True(def.IsSchemaLoaded);
        }

        [Fact]
        public async Task LoadSchema()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var def = new MySchemaDefinition();
            var schema = await store.Schemas.New(def).CreateAsync();
            Assert.NotNull(schema);
            Assert.Equal(1, schema.GetSchemaInfos().Count());
            Assert.Equal(2, store.Schemas.All().Count());
        }

        [Fact]
        public async Task UnloadSchema()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var def = new MySchemaDefinition();
            var schema = await store.Schemas.New(def).CreateAsync();
            Assert.NotNull(schema);
            store.Schemas.Unload(schema);
            Assert.Equal(1, store.Schemas.All().Count());
        }

        [Fact]
        public async Task UnloadPrimitivesSchemaMustFailed()
        {
            var domain = await StoreBuilder.CreateDomain<MySchemaDefinition>("Test");
            var store = domain.Store;
            var schema = store.Schemas.All().First();
            Assert.NotNull(schema);
            Assert.IsType<PrimitivesSchema>(schema);
            Assert.Throws<Exception>(() => store.Schemas.Unload(schema));
        }
    }
}