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
            : base("Hyperstore.Tests.Relationships", DomainBehavior.Observable)
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
        private IEnumerable<Product> _products2;

        public Customer(IDomainModel domain = null)
            : base(domain)
        {

        }

        protected override void Initialize(ISchemaElement metadata, IDomainModel domainModel)
        {
            base.Initialize(metadata, domainModel);
            var observable = metadata.Schema.Behavior == DomainBehavior.Observable;
            _products = observable ? (ICollection<Product>)new ObservableModelElementCollection<Product>(this, "CustomerReferencesProducts") : (ICollection<Product>)new ModelElementCollection<Product>(this, "CustomerReferencesProducts");
            _products2 = observable ? new ObservableModelElementList<Product>(this, "CustomerReferencesProducts") : new ModelElementList<Product>(this, "CustomerReferencesProducts");
            ((ModelElementList<Product>)_products2).WhereClause = item => item.Name.EndsWith("0");
        }

        public ICollection<Product> Products
        {
            get { return _products; }
        }

        public IEnumerable<Product> Products2
        {
            get { return _products2; }
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

        public Product(IDomainModel domain = null)
            : base(domain)
        {
        }
        protected override void Initialize(ISchemaElement metadata, IDomainModel domainModel)
        {
            base.Initialize(metadata, domainModel);
            var observable = metadata.Schema.Behavior == DomainBehavior.Observable;
            _customers = observable ? (ICollection<Customer>)new ObservableModelElementCollection<Customer>(this, "CustomerReferencesProducts", true) : (ICollection<Customer>)new ModelElementCollection<Customer>(this, "CustomerReferencesProducts", true);
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

    
    public class RelationshipTest : HyperstoreTestBase
    {
        [Fact]
        public async Task ManyToManyTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<RelationshipTestModel>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            store.DefaultSessionConfiguration.DefaultDomainModel = dm;
            int size = 10;

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

            Assert.Equal(5, customers[0].Products.Count);
            Assert.Equal(1, products[0].Customers.Count);

            customers[1].Products.Add(products[0]);
            customers[1].Products.Add(products[1]);
            customers[1].Products.Add(products[2]);
            customers[1].Products.Add(products[3]);
            customers[1].Products.Add(products[4]);

            Assert.Equal(5, customers[1].Products.Count);
            Assert.Equal(2, products[0].Customers.Count);

            products[0].Customers.Add(customers[2]);
            Assert.Equal(1, customers[2].Products.Count);
            Assert.Equal(3, products[0].Customers.Count);

            products[0].Customers.ToList();
            customers[0].Products.ToList();

            customers[0].Products.Remove(products[0]);
            Assert.Equal(4, customers[0].Products.Count);
            Assert.Equal(2, products[0].Customers.Count);

            products[0].Customers.Remove(customers[2]);
            Assert.Equal(0, customers[2].Products.Count);
            Assert.Equal(1, products[0].Customers.Count);
        }

        [Fact]
        public async Task ObservableManyToManyTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<RelationshipTestModel>().CreateAsync();
            var dm = await store.DomainModels.New().CreateAsync("Test");
            store.DefaultSessionConfiguration.DefaultDomainModel = dm;
            int size = 10;

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
                    customers[0].Products.Add(p);
                }

                s.AcceptChanges();
            }

            //customers[0].Products.Add(products[0]);
            //customers[0].Products.Add(products[1]);
            //customers[0].Products.Add(products[2]);
            //customers[0].Products.Add(products[3]);
            //customers[0].Products.Add(products[4]);

            Assert.Equal(size, customers[0].Products.Count);
            Assert.Equal(1, products[0].Customers.Count);

            customers[1].Products.Add(products[0]);
            customers[1].Products.Add(products[1]);
            customers[1].Products.Add(products[2]);
            customers[1].Products.Add(products[3]);
            customers[1].Products.Add(products[4]);

            Assert.Equal(5, customers[1].Products.Count);
            Assert.Equal(2, products[0].Customers.Count);

            products[0].Customers.Add(customers[2]);
            Assert.Equal(1, customers[2].Products.Count);
            Assert.Equal(3, products[0].Customers.Count);

            products[0].Customers.ToList();
            customers[0].Products.ToList();

            Assert.Equal(1, customers[0].Products2.Count());

            // Change property from the whereclause
            using (var session = store.BeginSession())
            {
                products[0].Name = "Test";
                session.AcceptChanges();
            }

            Assert.Equal(0, customers[0].Products2.Count());

            customers[0].Products.Remove(products[0]);
            Assert.Equal(size - 1, customers[0].Products.Count);
            Assert.Equal(2, products[0].Customers.Count);

            products[0].Customers.Remove(customers[2]);
            Assert.Equal(0, customers[2].Products.Count);
            Assert.Equal(1, products[0].Customers.Count);

        }
    }
}
