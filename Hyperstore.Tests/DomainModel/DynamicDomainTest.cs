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
using Hyperstore.Modeling.HyperGraph;
using System.Collections.Generic;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif


namespace Hyperstore.Tests
{
    [TestClass]
    public class DynamicDomainTest : HyperstoreTestBase
    {
        private class DynamicModelDefinition : SchemaDefinition
        {
            public static ISchemaEntity NamedElement { get; protected set; }
            public static IIndex ElementByNames { get; protected set; }
            public static ISchemaEntity Library { get; protected set; }
            public static ISchemaEntity Book { get; protected set; }
            public static ISchemaEntity Member { get; protected set; }
            public static ISchemaEntity Loan { get; protected set; }
            public static ISchemaRelationship LoanReferencesBook { get; protected set; }
            public static ISchemaRelationship LibraryHasBooks { get; protected set; }
            public static ISchemaRelationship LibraryHasMembers { get; protected set; }
            public static ISchemaRelationship LibraryHasLoans { get; protected set; }
            public static ISchemaRelationship LoanReferencesMember { get; protected set; }

            public DynamicModelDefinition()
                : base("Hyperstore.Tests")
            {
            }

            protected override void DefineSchema(ISchema metaModel)
            {
                NamedElement = new SchemaEntity(metaModel, "NamedElement");
                Library = new SchemaEntity(metaModel, "Library", NamedElement);
                Book = new SchemaEntity(metaModel, "Book");
                Member = new SchemaEntity(metaModel, "Member", NamedElement);
                Loan = new SchemaEntity(metaModel, "Loan");
                LoanReferencesBook = new SchemaRelationship("LoanReferencesBook", Loan, Book, Cardinality.ManyToOne, false, null, "Book");
                LibraryHasBooks = new SchemaRelationship("LibraryHasBooks", Library, Book, Cardinality.OneToMany, true, null, "Books");
                LibraryHasMembers = new SchemaRelationship("LibraryHasMembers", Library, Member, Cardinality.OneToMany, true, null, "Members");
                LibraryHasLoans = new SchemaRelationship("LibraryHasLoans", Library, Loan, Cardinality.OneToMany, true, null, "Loans");
                LoanReferencesMember = new SchemaRelationship("LoanReferencesMember", Loan, Member, Cardinality.ManyToOne, false, null, "Member");

                NamedElement.DefineProperty<string>("Name");
                Book.DefineProperty<string>("Title");
                Book.DefineProperty<int>("Copies");
            }

            protected override void OnSchemaLoaded(ISchema domainModel)
            {
                base.OnSchemaLoaded(domainModel);
                ElementByNames = domainModel.Indexes.CreateIndex(NamedElement, "ElementByNames", false, "Name");
            }
        }

        private async Task<IDomainModel> LoadDynamicDomain()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<DynamicModelDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            return domain;
        }

        [TestMethod]
        public async Task LoadDynamicDomainTest()
        {
            var dm = await LoadDynamicDomain();
            var schema = dm.Store.Schemas.Get("Hyperstore.Tests");

            Assert.AreEqual(13, schema.GetSchemaInfos().Count());
            Assert.AreEqual(10, schema.GetSchemaElements().Count());
            Assert.AreEqual(5, schema.GetSchemaEntities().Count());
            Assert.AreEqual(5, schema.GetSchemaRelationships().Count());
        }

        [TestMethod]
        public async Task CreateDynamicElementTest()
        {
            var domain = await LoadDynamicDomain();

            dynamic x = null;
            using (var session = domain.Store.BeginSession())
            {
                x = new DynamicModelEntity(domain, "Library");
                session.AcceptChanges();
            }

            Assert.IsNotNull(x);
        }

        [TestMethod]
        public async Task CreateDynamicElementTest2()
        {
            var domain = await LoadDynamicDomain();

            dynamic x = null;
            using (var session = domain.Store.BeginSession())
            {
                x = domain.CreateEntity(domain.Store.GetSchemaEntity("Library"));
                x.Name = "LIB";
                session.AcceptChanges();
            }

            Assert.IsNotNull(x);
            Assert.AreEqual("LIB", x.Name);
        }

        [TestMethod]
        public async Task DynamicPropertyTest()
        {
            var domain = await LoadDynamicDomain();

            dynamic x = null;
            using (var session = domain.Store.BeginSession())
            {
                x = new DynamicModelEntity(domain, "Library");
                x.Name = "Library";
                session.AcceptChanges();
            }

            Assert.IsNotNull(x);
            Assert.AreEqual("Library", x.Name);
        }

        [TestMethod]
        public async Task DynamicReferenceTest()
        {
            var domain = await LoadDynamicDomain();

            dynamic lib = null;
            dynamic loan = null;
            using (var session = domain.Store.BeginSession())
            {
                lib = new DynamicModelEntity(domain, "Library");
                lib.Name = "Library";

                var book = new DynamicModelEntity(domain, "Book");
                lib.Books.Add(book);

                loan = new DynamicModelEntity(domain, "Loan");
                loan.Book = book;

                session.AcceptChanges();
            }

            Assert.IsNotNull(lib);
            Assert.IsNotNull(loan.Book);
            Assert.IsNotNull(((IEnumerable<IModelElement>)lib.Books).FirstOrDefault());
        }
    }
}
