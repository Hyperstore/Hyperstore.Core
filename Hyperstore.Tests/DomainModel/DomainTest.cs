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


namespace Hyperstore.Tests
{
    [TestClass]
    public class DomainModelTest : HyperstoreTestBase
    {
        [TestMethod]
        public async Task ReferenceInRelationshipTest()
        {
            var store = StoreBuilder.New().Create();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test"); 

            XReferencesY rel = null;
            using( var s = store.BeginSession() )
            {
                var start = new XExtendsBaseClass(dm);
                var end = new YClass(dm);
                rel = new XReferencesY( start, end );
                rel.YRelation = end;
                s.AcceptChanges();
            }

            Assert.IsNotNull( rel );
            Assert.IsNotNull( rel.YRelation );
        }


        [TestMethod]
        public async Task SetReferenceTest()
        {
            var store = StoreBuilder.New().Create();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test"); 
            XExtendsBaseClass x = null;
            YClass y = null;
            using( var s = store.BeginSession() )
            {
                x = new XExtendsBaseClass( dm );
                y = new YClass( dm ) { Name = "1" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, y );
            Assert.AreEqual( x.YRelation.X, x );
            Assert.AreEqual( y.X, x );
            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.AreEqual( rel.Start, x );
            Assert.AreEqual( rel.End, y );
            
            using( var s = store.BeginSession() )
            {
                y = new YClass( dm ) { Name = "2" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, y );
            Assert.AreEqual( x.YRelation.X, x );
            Assert.AreEqual( x.GetRelationships<XReferencesY>().Count(), 1 );

            rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.AreEqual( rel.Start, x );
            Assert.AreEqual( rel.End, y );
        }

        [TestMethod]
        public async Task SetReferenceFromOppositeTest()
        {
            var store = StoreBuilder.New().Create();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test"); 
            XExtendsBaseClass x = null;
            YClass y = null;
            using( var s = store.BeginSession() )
            {
                x = new XExtendsBaseClass( dm );
                y = new YClass( dm ) { Name = "1" };
                y.X = x;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, y );
            Assert.AreEqual( x.YRelation.X, x );
            Assert.AreEqual( y.X, x );
            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.AreEqual( rel.Start, x );
            Assert.AreEqual( rel.End, y );

            using( var s = store.BeginSession() )
            {
                y = new YClass( dm ) { Name = "2" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, y );
            Assert.AreEqual( x.YRelation.X, x );
            Assert.AreEqual( x.GetRelationships<XReferencesY>().Count(), 1 );

            rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            Assert.AreEqual( rel.Start, x );
            Assert.AreEqual( rel.End, y );
        }

        [TestMethod]
        public async Task PropertyChangedOnSetReferenceTest()
        {
            var store = StoreBuilder.New().Create();
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

            Assert.AreEqual(3, yrelationChanges);
            Assert.AreEqual(3, allPropertychanges);
        }

        [TestMethod]
        public async Task SetReferenceToNullTest()
        {
            var store = StoreBuilder.New().Create();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test"); 
            XExtendsBaseClass x = null;
            YClass y = null;
            using( var s = store.BeginSession() )
            {
                x = new XExtendsBaseClass( dm );
                y = new YClass( dm ) { Name = "1" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using( var s = store.BeginSession() )
            {
                x.YRelation = null;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, null );
            Assert.AreEqual( y.X, null );
            Assert.AreEqual( x.GetRelationships<XReferencesY>().Count(), 0 );
        }

        [TestMethod]
        public async Task SetReferenceToNullFromOppositeTest()
        {
            var store = StoreBuilder.New().Create();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test"); 
            XExtendsBaseClass x = null;
            YClass y = null;
            using( var s = store.BeginSession() )
            {
                x = new XExtendsBaseClass( dm );
                y = new YClass( dm ) { Name = "1" };
                x.YRelation = y;
                s.AcceptChanges();
            }

            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using( var s = store.BeginSession() )
            {
                y.X = null;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, null );
            Assert.AreEqual( y.X, null );
            Assert.AreEqual( x.GetRelationships<XReferencesY>().Count(), 0 );
        }


        [TestMethod]
        public async Task SetReferenceToNullFromOpposite2Test()
        {
            var store = StoreBuilder.New().Create();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test"); 
            XExtendsBaseClass x = null;
            YClass y = null;
            using( var s = store.BeginSession() )
            {
                x = new XExtendsBaseClass( dm );
                y = new YClass( dm ) { Name = "1" };
                new XReferencesY( x, y );
                s.AcceptChanges();
            }

            var rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using( var s = store.BeginSession() )
            {
                y.X = null;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, null );
            Assert.AreEqual( y.X, null );
            Assert.AreEqual( x.GetRelationships<XReferencesY>().Count(), 0 );

            using( var s = store.BeginSession() )
            {
                new XReferencesY( x, y );
                s.AcceptChanges();
            }

            rel = x.GetRelationships<XReferencesY>().FirstOrDefault();
            using( var s = store.BeginSession() )
            {
                x.YRelation = null;
                s.AcceptChanges();
            }

            Assert.AreEqual( x.YRelation, null );
            Assert.AreEqual( y.X, null );
            Assert.AreEqual( x.GetRelationships<XReferencesY>().Count(), 0 );

        }
    }
}
