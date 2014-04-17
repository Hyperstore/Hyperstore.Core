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
using System.Collections.Generic;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif


namespace Hyperstore.Tests.Relationships
{
    // Définition d'un modèle avec de relations many to many
    public class RelationshipTestModel : SchemaDefinition
    {
        public RelationshipTestModel()
            : base("Hyperstore.Tests.Relationships")
        {
        }

        protected override void DefineSchema(ISchema schema)
        {
            ISchemaEntity customer = new SchemaEntity<Customer>(schema);
            customer.DefineProperty<string>("Name");
            ISchemaEntity product = new SchemaEntity<Product>(schema);
            product.DefineProperty<string>("Name");
            ISchemaRelationship rel = new SchemaRelationship("CustomerReferencesProducts", customer, product, Cardinality.ManyToMany);
        }
    }

    public class Customer : ModelEntity
    {
        private ICollection<Product> _products;
        public Customer(IDomainModel domain=null):base(domain)
        {
           
        }

        protected override void Initialize(ISchemaElement metadata, IDomainModel domainModel)
        {
            base.Initialize(metadata, domainModel);
            var observable = Session.Current.GetContextInfo<bool>("observable");
            _products = observable ? new ObservableModelElementCollection<Product>(this, "CustomerReferencesProducts") : new ModelElementCollection<Product>(this, "CustomerReferencesProducts");
        }

        public ICollection<Product> Products
        {
            get { return _products; }
        }
        public string Name
        {
            get { return GetPropertyValue<string>("Name"); }
            set { SetPropertyValue("Name", value); }
        }
    }

    public class Product : ModelEntity
    {
        private ICollection<Customer> _customers;

        public Product(IDomainModel domain=null)
            : base(domain)
        {
        }
        protected override void Initialize(ISchemaElement metadata, IDomainModel domainModel)
        {
            base.Initialize(metadata, domainModel);
            var observable = Session.Current.GetContextInfo<bool>("observable");
            _customers = observable ? new ObservableModelElementCollection<Customer>(this, "CustomerReferencesProducts", true) : new ModelElementCollection<Customer>(this, "CustomerReferencesProducts", true);
        }

        public ICollection<Customer> Customers
        {
            get { return _customers; }
        }
        public string Name
        {
            get { return GetPropertyValue<string>("Name"); }
            set { SetPropertyValue("Name", value); }
        }
    }

    [TestClass]
    public class RelationshipTest : HyperstoreTestBase
    {
        [TestMethod]
        public async Task ManyToManyTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new RelationshipTestModel());
            var dm = await store.CreateDomainModelAsync("Test");
            store.DefaultSessionConfiguration.DefaultDomainModel = dm;
            int size=10;

            var customers = new Customer[size];
            using (var s = store.BeginSession())
            {
                for (int i = 0; i < size; i++)
                {
                    var c = new Customer();
                    c.Name = "Customer " + i.ToString();
                    customers[i] = c;
                }
                s.AcceptChanges();
            }

            var products = new Product[size];
            using (var s = store.BeginSession())
            {
                for (int i = 0; i < size; i++)
                {
                    var p = new Product();
                    p.Name = "Product " + i.ToString();
                    products[i] = p;
                }
                s.AcceptChanges();
            }

            customers[0].Products.Add(products[0]);
            customers[0].Products.Add(products[1]);
            customers[0].Products.Add(products[2]);
            customers[0].Products.Add(products[3]);
            customers[0].Products.Add(products[4]);

            Assert.AreEqual(5, customers[0].Products.Count);
            Assert.AreEqual(1, products[0].Customers.Count);

            customers[1].Products.Add(products[0]);
            customers[1].Products.Add(products[1]);
            customers[1].Products.Add(products[2]);
            customers[1].Products.Add(products[3]);
            customers[1].Products.Add(products[4]);

            Assert.AreEqual(5, customers[1].Products.Count);
            Assert.AreEqual(2, products[0].Customers.Count);

            products[0].Customers.Add(customers[2]);
            Assert.AreEqual(1, customers[2].Products.Count);
            Assert.AreEqual(3, products[0].Customers.Count);

            products[0].Customers.ToList();
            customers[0].Products.ToList();

            customers[0].Products.Remove(products[0]);
            Assert.AreEqual(4, customers[0].Products.Count);
            Assert.AreEqual(2, products[0].Customers.Count);

            products[0].Customers.Remove(customers[2]);
            Assert.AreEqual(0, customers[2].Products.Count);
            Assert.AreEqual(1, products[0].Customers.Count);
        }

        [TestMethod]
        public async Task ObservableManyToManyTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new RelationshipTestModel());
            var dm = await store.CreateDomainModelAsync("Test");
            store.DefaultSessionConfiguration.DefaultDomainModel = dm;
            int size = 10;

            var customers = new Customer[size];
            using (var s = store.BeginSession())
            {
                Session.Current.SetContextInfo("observable", true);
                for (int i = 0; i < size; i++)
                {
                    var c = new Customer();
                    c.Name = "Customer " + i.ToString();
                    customers[i] = c;
                }
                s.AcceptChanges();
            }

            var products = new Product[size];
            using (var s = store.BeginSession())
            {
                Session.Current.SetContextInfo("observable", true);
                for (int i = 0; i < size; i++)
                {
                    var p = new Product();
                    p.Name = "Product " + i.ToString();
                    products[i] = p;
                }
                s.AcceptChanges();
            }

            customers[0].Products.Add(products[0]);
            customers[0].Products.Add(products[1]);
            customers[0].Products.Add(products[2]);
            customers[0].Products.Add(products[3]);
            customers[0].Products.Add(products[4]);

            Assert.AreEqual(5, customers[0].Products.Count);
            Assert.AreEqual(1, products[0].Customers.Count);

            customers[1].Products.Add(products[0]);
            customers[1].Products.Add(products[1]);
            customers[1].Products.Add(products[2]);
            customers[1].Products.Add(products[3]);
            customers[1].Products.Add(products[4]);

            Assert.AreEqual(5, customers[1].Products.Count);
            Assert.AreEqual(2, products[0].Customers.Count);

            products[0].Customers.Add(customers[2]);
            Assert.AreEqual(1, customers[2].Products.Count);
            Assert.AreEqual(3, products[0].Customers.Count);

            products[0].Customers.ToList();
            customers[0].Products.ToList();

            customers[0].Products.Remove(products[0]);
            Assert.AreEqual(4, customers[0].Products.Count);
            Assert.AreEqual(2, products[0].Customers.Count);

            products[0].Customers.Remove(customers[2]);
            Assert.AreEqual(0, customers[2].Products.Count);
            Assert.AreEqual(1, products[0].Customers.Count);
        }    
    }
}
