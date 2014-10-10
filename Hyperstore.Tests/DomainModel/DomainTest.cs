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


namespace Hyperstore.Tests
{
    
    public class DomainModelTest : HyperstoreTestBase
    {
        [Fact]
        public async Task ReferenceInRelationshipTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            XReferencesY rel = null;
            using (var s = store.BeginSession())
            {
                var start = new XExtendsBaseClass(dm);
                var end = new YClass(dm);
                rel = new XReferencesY(start, end);
                rel.YRelation = end;
                s.AcceptChanges();
            }

            Assert.NotNull(rel);
            Assert.NotNull(rel.YRelation);
        }


        [Fact]
        public async Task SetReferenceTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            XExtendsBaseClass x = null;
            YClass y = null;
            using (var s = store.BeginSession())
            {
                x = new XExtendsBaseClass(dm);
                y = new YClass(dm) { Name = "1" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, y);
            Assert.Equal(x.YRelation.X, x);
            Assert.Equal(y.X, x);
            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.Equal(rel.Start, x);
            Assert.Equal(rel.End, y);

            using (var s = store.BeginSession())
            {
                y = new YClass(dm) { Name = "2" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, y);
            Assert.Equal(x.YRelation.X, x);
            Assert.Equal(x.GetRelationships<XReferencesY>().Count(), 1);

            rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.Equal(rel.Start, x);
            Assert.Equal(rel.End, y);
        }

        [Fact]
        public async Task SetReferenceFromOppositeTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            XExtendsBaseClass x = null;
            YClass y = null;
            using (var s = store.BeginSession())
            {
                x = new XExtendsBaseClass(dm);
                y = new YClass(dm) { Name = "1" };
                y.X = x;
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, y);
            Assert.Equal(x.YRelation.X, x);
            Assert.Equal(y.X, x);
            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.Equal(rel.Start, x);
            Assert.Equal(rel.End, y);

            using (var s = store.BeginSession())
            {
                y = new YClass(dm) { Name = "2" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, y);
            Assert.Equal(x.YRelation.X, x);
            Assert.Equal(x.GetRelationships<XReferencesY>().Count(), 1);

            rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.Equal(rel.Start, x);
            Assert.Equal(rel.End, y);
        }

        [Fact]
        public async Task PropertyChangedOnSetReferenceTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().UsingIdGenerator(r => new LongIdGenerator()).CreateAsync("Test");
            XExtendsBaseClass x = null;
            YClass y = null;

            var yrelationChanges = 0;
            var allPropertychanges = 0;

            using (var s = store.BeginSession())
            {
                x = new XExtendsBaseClass(dm);
                s.AcceptChanges();
            }

            x.PropertyChanged += (sender, e) =>
                    {
                        allPropertychanges++;
                        if (e.PropertyName == "YRelation") yrelationChanges++;
                    };

            using (var s = store.BeginSession())
            {
                y = new YClass(dm) { Name = "1" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            using (var s = store.BeginSession())
            {
                y = new YClass(dm) { Name = "2" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using (var s = store.BeginSession())
            {
                x.YRelation = null;
                s.AcceptChanges();
            }

            Assert.Equal(3, yrelationChanges);
            Assert.Equal(3, allPropertychanges);
        }

        [Fact]
        public async Task SetReferenceToNullTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            XExtendsBaseClass x = null;
            YClass y = null;
            using (var s = store.BeginSession())
            {
                x = new XExtendsBaseClass(dm);
                y = new YClass(dm) { Name = "1" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using (var s = store.BeginSession())
            {
                x.YRelation = null;
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, null);
            Assert.Equal(((IModelElement)y).Status, ModelElementStatus.Disposed);
            Assert.Equal(x.GetRelationships<XReferencesY>().Count(), 0);
        }

        [Fact]
        public async Task SetReferenceToNullFromOppositeTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            XExtendsBaseClass x = null;
            YClass y = null;
            using (var s = store.BeginSession())
            {
                x = new XExtendsBaseClass(dm);
                y = new YClass(dm) { Name = "1" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using (var s = store.BeginSession())
            {
                y.X = null;
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, null);
            Assert.Equal(((IModelElement)y).Status, ModelElementStatus.Disposed);
            Assert.Equal(x.GetRelationships<XReferencesY>().Count(), 0);
        }


        [Fact]
        public async Task SetReferenceToNullFromOpposite2Test()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            XExtendsBaseClass x = null;
            YClass y = null;
            using (var s = store.BeginSession())
            {
                x = new XExtendsBaseClass(dm);
                y = new YClass(dm) { Name = "1" };
                new XReferencesY(x, y);
                s.AcceptChanges();
            }

            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using (var s = store.BeginSession())
            {
                y.X = null; // TODO Since it's an embedded relationship and y is the opposite, y is deleted. Is it the wright behavior ???
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, null);
            Assert.Equal(x.GetRelationships<XReferencesY>().Count(), 0);

            using (var s = store.BeginSession())
            {
                y = new YClass(dm) { Name = "1" };
                new XReferencesY(x, y);
                s.AcceptChanges();
            }

            rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using (var s = store.BeginSession())
            {
                x.YRelation = null;
                s.AcceptChanges();
            }

            Assert.Equal(x.YRelation, null);
            Assert.Equal(((IModelElement)y).Status, ModelElementStatus.Disposed);
            Assert.Equal(x.GetRelationships<XReferencesY>().Count(), 0);

        }

        [Fact]
        public async Task EmbeddedRelationship()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");

            Library lib;
            Book b;

            using (var s = store.BeginSession())
            {
                lib = new Library(dm);
                lib.Name = "Library";

                b = new Book(dm);
                b.Title = "book";
                b.Copies = 1;

                lib.Books.Add(b);
                s.AcceptChanges();
            }

            using (var s = store.BeginSession())
            {
                lib.Books.Remove(b);
                s.AcceptChanges();
            }

            Assert.Null(dm.GetElement<Book>(((IModelElement)b).Id));
            Assert.Equal(0, lib.Books.Count());
        }
    }
}
