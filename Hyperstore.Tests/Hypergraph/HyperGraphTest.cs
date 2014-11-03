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
using Hyperstore.Modeling;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using System.Threading.Tasks;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Tests.Model;
using Hyperstore.Tests.MemoryStore;
using Hyperstore.Modeling.Traversal;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.HyperGraph
{
    
    public class HyperGraphTest : HyperstoreTestBase
    {
        [Fact]
        
        public async Task InitTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var Graph = domain.Resolve<IHyperGraph>();
            Assert.NotNull(Graph);
        }

        /// <summary>
        ///A test for AddVertex
        ///</summary>
        [Fact]
        
        public async Task AddElementTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var Graph = domain.Resolve<IHyperGraph>();
            var aid = Id(1);
            using (var session = domain.Store.BeginSession())
            {
                Graph.CreateEntity(aid, store.PrimitivesSchema.SchemaEntitySchema);

                Assert.NotNull(Graph.GetElement(aid, store.PrimitivesSchema.SchemaEntitySchema));
                session.AcceptChanges();
            }
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.NotNull(Graph.GetElement(aid, store.PrimitivesSchema.SchemaEntitySchema));
            }
        }

        [Fact]
        
        public async Task AddElementOutOfScopeTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var Graph = domain.Resolve<IHyperGraph>();
            var aid = Id(1);
            using (var session = domain.Store.BeginSession())
            {
                Graph.CreateEntity(aid, store.PrimitivesSchema.SchemaEntitySchema);
                session.AcceptChanges();
            }
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.NotNull(Graph.GetElement(aid, store.PrimitivesSchema.SchemaEntitySchema));
            }
        }

        [Fact]
        
        public async Task AddElementRollbackTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test"); 

            var Graph = domain.Resolve<IHyperGraph>();
            var aid = Id(1);
            using (var session = domain.Store.BeginSession())
            {
                Graph.CreateEntity(aid, store.PrimitivesSchema.SchemaEntitySchema);
                Assert.NotNull(Graph.GetElement(aid, store.PrimitivesSchema.SchemaEntitySchema));
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.Null(Graph.GetElement(aid, store.PrimitivesSchema.SchemaEntitySchema));
            }
        }

        [Fact]
        
        public async Task RemoveElementTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test"); 

            CreateGraph(domain);
            var Graph = domain.Resolve<IHyperGraph>();

            var aid = Id(1);
            var metadata = schema.Definition.XExtendsBaseClass;
            var mel = domain.GetElement(aid, metadata);
            var namePropertyMetadata = mel.SchemaInfo.GetProperty("Name");

                var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
                Assert.Null(prop);

            using (var session = domain.Store.BeginSession())
            {
                Assert.NotEqual(0, Graph.SetPropertyValue(mel, namePropertyMetadata, "am", null).CurrentVersion);
                session.AcceptChanges();
            }

                Assert.True(Graph.GetElement(aid, metadata) != null);
             //   Assert.True(Graph.GetElement(aid.CreateAttributeIdentity("Name"), null, true) != null);

            using (var session = domain.Store.BeginSession())
            {
                Graph.RemoveEntity(aid, metadata, true);
                session.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.False(Graph.GetElement(aid, metadata) != null);
              //  Assert.False(Graph.GetElement(aid.CreateAttributeIdentity("Name"), null, true) != null);
            }
        }

        [Fact]
        
        public async Task RollbackPropertyTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            CreateGraph(domain);
            var aid = Id(1);

            var Graph = domain.Resolve<IHyperGraph>();
            ISchemaEntity metadata;
            IModelElement mel;
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                metadata = schema.Definition.XExtendsBaseClass;
                mel = domain.GetElement(aid, metadata);
            }

            var version = 0L;
            using (var session = domain.Store.BeginSession())
            {
                version = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion;
                Assert.NotEqual(0, version);
                session.AcceptChanges();
            }

            mel = domain.GetElement(aid, metadata);
            var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
            Assert.NotNull(prop);
            Assert.Equal("am", prop.Value);

            await Task.Delay(20); // Délai mini entre diffèrents Ticks de UtcNow 

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", null).CurrentVersion;
                Assert.True(v > version);
                // Rollback
            }

            prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
            Assert.NotNull(prop);
            Assert.Equal("am", prop.Value);
            Assert.Equal(version, prop.CurrentVersion);

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", null).CurrentVersion;
                Assert.True(v > version);
                session.AcceptChanges();
                version = v;
            }
            await Task.Delay(20); // Délai mini entre diffèrents Ticks de UtcNow 

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion;
                Assert.True(v > version); 
                session.AcceptChanges();
            }
        }

        //[Fact]
        //
        //public async Task ConflictPropertyTest()
        //{
        //    var store = await StoreBuilder.Init().CreateStore();
        //    await store.Schemas.New<TestDomainDefinition>().CreateAsync();
        //    var domain = await store.DomainModels.New().CreateAsync("Test");

        //    CreateGraph(domain);
        //    var aid = Id(1);

        //    var Graph = domain.Resolve<IHyperGraph>();
        //    ISchemaEntity metadata;
        //    IModelElement mel;
        //    metadata = TestDomainDefinition.XExtendsBaseClass;
        //    mel = domain.GetElement(aid, metadata);

        //    var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
        //    Assert.Null(prop);

        //    using (var session = domain.Store.BeginSession())
        //    {
        //        Assert.Equal(0, Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion);
        //        session.AcceptChanges();
        //    }

        //    using (var session = domain.Store.BeginSession())
        //    {
        //        Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", 1);
        //        session.AcceptChanges();
        //    }

        //    Assert.ThrowsAsync<ConflictException>(() =>
        //        {
        //            using (var session = domain.Store.BeginSession())
        //            {
        //                Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am3", 1);
        //                session.AcceptChanges();
        //            }
        //        });
        //}

        [Fact]
        
        public async Task GetPropertyTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            CreateGraph(domain);
            var aid = Id(1);

            var Graph = domain.Resolve<IHyperGraph>();
            ISchemaEntity metadata;
            IModelElement mel;
            metadata = schema.Definition.XExtendsBaseClass;
            mel = domain.GetElement(aid, metadata);

            var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
            Assert.Null(prop);

            var version = 0L;
            using (var session = domain.Store.BeginSession())
            {
                version = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion;
                Assert.NotEqual(0, version);
                session.AcceptChanges();
            }

            await Task.Delay(20); // Délai mini entre diffèrents Ticks de UtcNow 

            prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
            Assert.NotNull(prop);
            Assert.Equal("am", prop.Value);
            Assert.Equal(version, prop.CurrentVersion);

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", null).CurrentVersion;
                Assert.True(v > version);
                session.AcceptChanges();
                version = v;
            }

            prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
            Assert.NotNull(prop);
            Assert.Equal("am2", prop.Value);
            Assert.Equal(version, prop.CurrentVersion);
        }

        [Fact]
        
        public async Task GetPropertyTest2()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            CreateGraph(domain);
            var aid = Id(1);

            var Graph = domain.Resolve<IHyperGraph>();
            ISchemaEntity metadata;
            IModelElement mel;
            metadata = schema.Definition.XExtendsBaseClass;
            mel = domain.GetElement(aid, metadata);
            var gate = new System.Threading.ManualResetEvent(false);

            var factory = new TaskFactory();
            var t1 = factory.StartNew(() =>
            {
                using (var session = domain.Store.BeginSession())
                {
                    Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null);
                    gate.Set();
                    session.AcceptChanges();
                }
            });

            var t2 = factory.StartNew(() =>
            {
                using (var session = domain.Store.BeginSession())
                {
                    gate.WaitOne();

                    Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", null);
                    session.AcceptChanges();
                }
            });

            Task.WaitAll(t1, t2);

            var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
            Assert.NotNull(prop);
            Assert.Equal("am2", prop.Value);
        }

  

        //[Fact]
        //public void AddEdgeTest()
        //{
        //    var store = await StoreBuilder.Init().CreateStore();

        //    var domain = store.LoadDomainModelAsync("Test1");
        //    var id1 = new Identity("Test1", "1");
        //    var id2 = new Identity("Test1", "2");
        //    var Graph = ((domain)domain).GetInnerGraph() as HyperGraph;

        //    using (var tx = new System.Transactions.TransactionScope())
        //    {
        //        Graph.AddElement(id1, store.PrimitivesSchema.MetaClass.Id);
        //        var a = store.GetElement(id1, store.PrimitivesSchema.MetaClass.Id);
        //        Graph.AddElement(id2, store.PrimitivesSchema.MetaClass.Id);
        //        var b = store.GetElement(id2, store.PrimitivesSchema.MetaClass.Id);
        //        var eid = new Identity("Test", "10");

        //        store.ProcessCommand(new Hyperstore.Modeling.Commands.AddRelationshipCommand(store.PrimitivesSchema.ModelRelationshipMetaClass.Id, a, b, eid));

        //        Assert.NotNull(Graph.GetElement(eid, store.PrimitivesSchema.ModelRelationshipMetaClass.Id));

        //        var nodea = Graph.GetElement(a.Id, a.Metadata.Id);
        //        Assert.NotNull(nodea);
        //        var nodeb = Graph.GetElement(b.Id, b.Metadata.Id);
        //        Assert.NotNull(nodeb);

        //        Assert.Equal(1, nodea.GetEdges(Direction.Outgoing).Count());
        //        Assert.Equal(0, nodea.GetEdges(Direction.Incoming).Count());
        //        Assert.Equal(1, nodeb.GetEdges(Direction.Incoming).Count());
        //        Assert.Equal(0, nodeb.GetEdges(Direction.Outgoing).Count());

        //        Assert.True(nodea.GetEdges(Direction.Outgoing).First().OutId == b.Id);
        //        Assert.True(nodea.GetEdges(Direction.Outgoing).First().OutMetadataId == b.Metadata.Id);
        //        Assert.True(nodeb.GetEdges(Direction.Incoming).First().OutId == a.Id);
        //        Assert.True(nodeb.GetEdges(Direction.Incoming).First().OutMetadataId == a.Metadata.Id);
        //        tx.Complete();
        //    }
        //}

        //[Fact]
        //public void RemoveEdgeTest()
        //{
        //    var store = await StoreBuilder.Init().CreateStore();

        //    var domain = store.LoadDomainModelAsync("Test1");
        //    var id1 = new Identity("Test1", "1");
        //    var id2 = new Identity("Test1", "2");
        //    var Graph = ((domain)domain).GetInnerGraph() as HyperGraph;

        //    using (var tx = new System.Transactions.TransactionScope())
        //    {
        //        Graph.AddElement(id1, store.PrimitivesSchema.MetaClass.Id);
        //        var a = store.GetElement(id1, store.PrimitivesSchema.MetaClass.Id);
        //        Graph.AddElement(id2, store.PrimitivesSchema.MetaClass.Id);
        //        var b = store.GetElement(id2, store.PrimitivesSchema.MetaClass.Id);
        //        var eid = new Identity("Test", "10");

        //        store.ProcessCommand(new Hyperstore.Modeling.Commands.AddRelationshipCommand(store.PrimitivesSchema.ModelRelationshipMetaClass.Id, a, b, eid));

        //        Assert.NotNull(Graph.GetNode(eid, store.PrimitivesSchema.ModelRelationshipMetaClass.Id));

        //        var nodea = Graph.GetNode(a.Id, a.Metadata.Id);
        //        Assert.NotNull(nodea);
        //        var nodeb = Graph.GetNode(b.Id, b.Metadata.Id);
        //        Assert.NotNull(nodeb);

        //        Assert.Equal(1, nodea.GetEdges(Direction.Outgoing).Count());
        //        Assert.Equal(0, nodea.GetEdges(Direction.Incoming).Count());
        //        Assert.Equal(1, nodeb.GetEdges(Direction.Incoming).Count());
        //        Assert.Equal(0, nodeb.GetEdges(Direction.Outgoing).Count());

        //        Graph.RemoveRelationship(eid, store.PrimitivesSchema.ModelRelationshipMetaClass);
        //        Assert.Null(Graph.GetNode(eid, store.PrimitivesSchema.ModelRelationshipMetaClass));

        //        nodea = Graph.GetNode(a.Id, a.Metadata.Id);
        //        nodeb = Graph.GetNode(b.Id, b.Metadata.Id);

        //        Assert.Equal(0, nodea.GetEdges(Direction.Outgoing).Count());
        //        Assert.Equal(0, nodea.GetEdges(Direction.Incoming).Count());
        //        Assert.Equal(0, nodeb.GetEdges(Direction.Incoming).Count());
        //        Assert.Equal(0, nodeb.GetEdges(Direction.Outgoing).Count());

        //        tx.Complete();
        //    }
        //}

        [Fact]
        
        public async Task MultiGraphTest()
        {
            var store = await StoreBuilder.New().CreateAsync();

            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test1");
            var id1 = new Identity("Test1", "1");

            var dm2 = await store.DomainModels.New().CreateAsync("Test2");
            var id2 = new Identity("Test2", "1");

            using (var session = domain.Store.BeginSession())
            {
                session.Execute(new Hyperstore.Modeling.Commands.AddEntityCommand(domain, store.PrimitivesSchema.SchemaEntitySchema, id1));
                session.Execute(new Hyperstore.Modeling.Commands.AddEntityCommand(dm2, store.PrimitivesSchema.SchemaEntitySchema, id2));

                var source = store.GetElement(id1, store.PrimitivesSchema.SchemaEntitySchema);
                var target = store.GetElement(id2, store.PrimitivesSchema.SchemaEntitySchema);
                session.Execute(new Hyperstore.Modeling.Commands.AddRelationshipCommand(store.PrimitivesSchema.ModelRelationshipSchema, source, target, new Identity("Test1", "10")));
                session.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                var src = store.GetElement(id1, store.PrimitivesSchema.SchemaEntitySchema);
                var end = store.GetElement(id2, store.PrimitivesSchema.SchemaEntitySchema);

                Assert.Equal(1, domain.GetRelationships(start: src).Count());
                Assert.Equal(0, dm2.GetRelationships(end: end).Count());

                Assert.NotNull(domain.GetRelationships(start: src).FirstOrDefault());
                Assert.Equal("10", domain.GetRelationships(start: src).First().Id.Key);
                Assert.Equal(id2, domain.GetRelationships(start: src).First().End.Id);
            }
        }


    }
}
