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
                LoanReferencesBook = new SchemaRelationship("LoanReferencesBook", Loan, Book, Cardinality.OneToOne, false);
                LibraryHasBooks = new SchemaRelationship("LibraryHasBooks", Library, Book, Cardinality.OneToMany, true);
                LibraryHasMembers = new SchemaRelationship("LibraryHasMembers", Library, Member, Cardinality.OneToMany, true);
                LibraryHasLoans = new SchemaRelationship("LibraryHasLoans", Library, Loan, Cardinality.OneToMany, true);
                LoanReferencesMember = new SchemaRelationship("LoanReferencesMember", Loan, Member, Cardinality.OneToOne, false);

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
            var store = new Store();
            await store.LoadSchemaAsync(new DynamicModelDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            return domain;
        }

        [TestMethod]
        public async Task LoadDynamicDomainTest()
        {
            var dm = await LoadDynamicDomain();
            var schema = dm.Store.Schemas.Last();

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
                x = domain.Store.CreateEntity(domain.Store.GetSchemaEntity("Library"), domain);
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
