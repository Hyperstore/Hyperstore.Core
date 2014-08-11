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
    [TestClass()]
    public class HyperGraphTest : HyperstoreTestBase
    {
        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task InitTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var Graph = domain.Resolve<IHyperGraph>();
            Assert.IsNotNull(Graph);
        }

        /// <summary>
        ///A test for AddVertex
        ///</summary>
        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task AddElementTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var Graph = domain.Resolve<IHyperGraph>();
            var aid = Id(1);
            using (var session = domain.Store.BeginSession())
            {
                Graph.CreateEntity(aid, PrimitivesSchema.SchemaEntitySchema);

                Assert.IsNotNull(Graph.GetElement(aid, PrimitivesSchema.SchemaEntitySchema));
                session.AcceptChanges();
            }
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNotNull(Graph.GetElement(aid, PrimitivesSchema.SchemaEntitySchema));
            }
        }

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task AddElementOutOfScopeTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            var Graph = domain.Resolve<IHyperGraph>();
            var aid = Id(1);
            using (var session = domain.Store.BeginSession())
            {
                Graph.CreateEntity(aid, PrimitivesSchema.SchemaEntitySchema);
                session.AcceptChanges();
            }
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNotNull(Graph.GetElement(aid, PrimitivesSchema.SchemaEntitySchema));
            }
        }

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task AddElementRollbackTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test"); 

            var Graph = domain.Resolve<IHyperGraph>();
            var aid = Id(1);
            using (var session = domain.Store.BeginSession())
            {
                Graph.CreateEntity(aid, PrimitivesSchema.SchemaEntitySchema);
                Assert.IsNotNull(Graph.GetElement(aid, PrimitivesSchema.SchemaEntitySchema));
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsNull(Graph.GetElement(aid, PrimitivesSchema.SchemaEntitySchema));
            }
        }

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task RemoveElementTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test"); 

            CreateGraph(domain);
            var Graph = domain.Resolve<IHyperGraph>();

            var aid = Id(1);
            var metadata = TestDomainDefinition.XExtendsBaseClass;
            var mel = domain.GetElement(aid, metadata);
            var namePropertyMetadata = mel.SchemaInfo.GetProperty("Name");

                var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo.GetProperty("Name"));
                Assert.IsNull(prop);

            using (var session = domain.Store.BeginSession())
            {
                Assert.AreNotEqual(0, Graph.SetPropertyValue(mel, namePropertyMetadata, "am", null).CurrentVersion);
                session.AcceptChanges();
            }

                Assert.IsTrue(Graph.GetElement(aid, metadata) != null);
             //   Assert.IsTrue(Graph.GetElement(aid.CreateAttributeIdentity("Name"), null, true) != null);

            using (var session = domain.Store.BeginSession())
            {
                Graph.RemoveEntity(aid, metadata, true);
                session.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                Assert.IsFalse(Graph.GetElement(aid, metadata) != null);
              //  Assert.IsFalse(Graph.GetElement(aid.CreateAttributeIdentity("Name"), null, true) != null);
            }
        }

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task RollbackPropertyTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            CreateGraph(domain);
            var aid = Id(1);

            var Graph = domain.Resolve<IHyperGraph>();
            ISchemaEntity metadata;
            IModelElement mel;
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                metadata = TestDomainDefinition.XExtendsBaseClass;
                mel = domain.GetElement(aid, metadata);
            }

            var version = 0L;
            using (var session = domain.Store.BeginSession())
            {
                version = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion;
                Assert.AreNotEqual(0, version);
                session.AcceptChanges();
            }

            mel = domain.GetElement(aid, metadata);
            var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo.GetProperty("Name"));
            Assert.IsNotNull(prop);
            Assert.AreEqual("am", prop.Value);

            await Task.Delay(20); // Délai mini entre diffèrents Ticks de UtcNow 

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", null).CurrentVersion;
                Assert.IsTrue(v > version);
                // Rollback
            }

            prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo.GetProperty("Name"));
            Assert.IsNotNull(prop);
            Assert.AreEqual("am", prop.Value);
            Assert.AreEqual(version, prop.CurrentVersion);

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", null).CurrentVersion;
                Assert.IsTrue(v > version);
                session.AcceptChanges();
                version = v;
            }
            await Task.Delay(20); // Délai mini entre diffèrents Ticks de UtcNow 

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion;
                Assert.IsTrue(v > version); 
                session.AcceptChanges();
            }
        }

        //[TestMethod()]
        //[TestCategory("Hypergraph")]
        //public async Task ConflictPropertyTest()
        //{
        //    var store = new Store();
        //    await store.LoadSchemaAsync(new TestDomainDefinition());
        //    var domain = await store.CreateDomainModelAsync("Test");

        //    CreateGraph(domain);
        //    var aid = Id(1);

        //    var Graph = domain.Resolve<IHyperGraph>();
        //    ISchemaEntity metadata;
        //    IModelElement mel;
        //    metadata = TestDomainDefinition.XExtendsBaseClass;
        //    mel = domain.GetElement(aid, metadata);

        //    var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo, mel.SchemaInfo.GetProperty("Name"));
        //    Assert.IsNull(prop);

        //    using (var session = domain.Store.BeginSession())
        //    {
        //        Assert.AreEqual(0, Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion);
        //        session.AcceptChanges();
        //    }

        //    using (var session = domain.Store.BeginSession())
        //    {
        //        Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", 1);
        //        session.AcceptChanges();
        //    }

        //    AssertHelper.ThrowsException<ConflictException>(() =>
        //        {
        //            using (var session = domain.Store.BeginSession())
        //            {
        //                Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am3", 1);
        //                session.AcceptChanges();
        //            }
        //        });
        //}

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task GetPropertyTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            CreateGraph(domain);
            var aid = Id(1);

            var Graph = domain.Resolve<IHyperGraph>();
            ISchemaEntity metadata;
            IModelElement mel;
            metadata = TestDomainDefinition.XExtendsBaseClass;
            mel = domain.GetElement(aid, metadata);

            var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo.GetProperty("Name"));
            Assert.IsNull(prop);

            var version = 0L;
            using (var session = domain.Store.BeginSession())
            {
                version = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am", null).CurrentVersion;
                Assert.AreNotEqual(0, version);
                session.AcceptChanges();
            }

            await Task.Delay(20); // Délai mini entre diffèrents Ticks de UtcNow 

            prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo.GetProperty("Name"));
            Assert.IsNotNull(prop);
            Assert.AreEqual("am", prop.Value);
            Assert.AreEqual(version, prop.CurrentVersion);

            using (var session = domain.Store.BeginSession())
            {
                var v = Graph.SetPropertyValue(mel, mel.SchemaInfo.GetProperty("Name"), "am2", null).CurrentVersion;
                Assert.IsTrue(v > version);
                session.AcceptChanges();
                version = v;
            }

            prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo.GetProperty("Name"));
            Assert.IsNotNull(prop);
            Assert.AreEqual("am2", prop.Value);
            Assert.AreEqual(version, prop.CurrentVersion);
        }

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task GetPropertyTest2()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");

            CreateGraph(domain);
            var aid = Id(1);

            var Graph = domain.Resolve<IHyperGraph>();
            ISchemaEntity metadata;
            IModelElement mel;
            metadata = TestDomainDefinition.XExtendsBaseClass;
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

            var prop = Graph.GetPropertyValue(mel.Id, mel.SchemaInfo.GetProperty("Name"));
            Assert.IsNotNull(prop);
            Assert.AreEqual("am2", prop.Value);
        }

  

        //[TestMethod()]
        //public void AddEdgeTest()
        //{
        //    var store = new Store();

        //    var domain = store.LoadDomainModelAsync("Test1");
        //    var id1 = new Identity("Test1", "1");
        //    var id2 = new Identity("Test1", "2");
        //    var Graph = ((domain)domain).GetInnerGraph() as HyperGraph;

        //    using (var tx = new System.Transactions.TransactionScope())
        //    {
        //        Graph.AddElement(id1, PrimitivesSchema.MetaClass.Id);
        //        var a = store.GetElement(id1, PrimitivesSchema.MetaClass.Id);
        //        Graph.AddElement(id2, PrimitivesSchema.MetaClass.Id);
        //        var b = store.GetElement(id2, PrimitivesSchema.MetaClass.Id);
        //        var eid = new Identity("Test", "10");

        //        store.ProcessCommand(new Hyperstore.Modeling.Commands.AddRelationshipCommand(PrimitivesSchema.ModelRelationshipMetaClass.Id, a, b, eid));

        //        Assert.IsNotNull(Graph.GetElement(eid, PrimitivesSchema.ModelRelationshipMetaClass.Id));

        //        var nodea = Graph.GetElement(a.Id, a.Metadata.Id);
        //        Assert.IsNotNull(nodea);
        //        var nodeb = Graph.GetElement(b.Id, b.Metadata.Id);
        //        Assert.IsNotNull(nodeb);

        //        Assert.AreEqual(1, nodea.GetEdges(Direction.Outgoing).Count());
        //        Assert.AreEqual(0, nodea.GetEdges(Direction.Incoming).Count());
        //        Assert.AreEqual(1, nodeb.GetEdges(Direction.Incoming).Count());
        //        Assert.AreEqual(0, nodeb.GetEdges(Direction.Outgoing).Count());

        //        Assert.IsTrue(nodea.GetEdges(Direction.Outgoing).First().OutId == b.Id);
        //        Assert.IsTrue(nodea.GetEdges(Direction.Outgoing).First().OutMetadataId == b.Metadata.Id);
        //        Assert.IsTrue(nodeb.GetEdges(Direction.Incoming).First().OutId == a.Id);
        //        Assert.IsTrue(nodeb.GetEdges(Direction.Incoming).First().OutMetadataId == a.Metadata.Id);
        //        tx.Complete();
        //    }
        //}

        //[TestMethod()]
        //public void RemoveEdgeTest()
        //{
        //    var store = new Store();

        //    var domain = store.LoadDomainModelAsync("Test1");
        //    var id1 = new Identity("Test1", "1");
        //    var id2 = new Identity("Test1", "2");
        //    var Graph = ((domain)domain).GetInnerGraph() as HyperGraph;

        //    using (var tx = new System.Transactions.TransactionScope())
        //    {
        //        Graph.AddElement(id1, PrimitivesSchema.MetaClass.Id);
        //        var a = store.GetElement(id1, PrimitivesSchema.MetaClass.Id);
        //        Graph.AddElement(id2, PrimitivesSchema.MetaClass.Id);
        //        var b = store.GetElement(id2, PrimitivesSchema.MetaClass.Id);
        //        var eid = new Identity("Test", "10");

        //        store.ProcessCommand(new Hyperstore.Modeling.Commands.AddRelationshipCommand(PrimitivesSchema.ModelRelationshipMetaClass.Id, a, b, eid));

        //        Assert.IsNotNull(Graph.GetNode(eid, PrimitivesSchema.ModelRelationshipMetaClass.Id));

        //        var nodea = Graph.GetNode(a.Id, a.Metadata.Id);
        //        Assert.IsNotNull(nodea);
        //        var nodeb = Graph.GetNode(b.Id, b.Metadata.Id);
        //        Assert.IsNotNull(nodeb);

        //        Assert.AreEqual(1, nodea.GetEdges(Direction.Outgoing).Count());
        //        Assert.AreEqual(0, nodea.GetEdges(Direction.Incoming).Count());
        //        Assert.AreEqual(1, nodeb.GetEdges(Direction.Incoming).Count());
        //        Assert.AreEqual(0, nodeb.GetEdges(Direction.Outgoing).Count());

        //        Graph.RemoveRelationship(eid, PrimitivesSchema.ModelRelationshipMetaClass);
        //        Assert.IsNull(Graph.GetNode(eid, PrimitivesSchema.ModelRelationshipMetaClass));

        //        nodea = Graph.GetNode(a.Id, a.Metadata.Id);
        //        nodeb = Graph.GetNode(b.Id, b.Metadata.Id);

        //        Assert.AreEqual(0, nodea.GetEdges(Direction.Outgoing).Count());
        //        Assert.AreEqual(0, nodea.GetEdges(Direction.Incoming).Count());
        //        Assert.AreEqual(0, nodeb.GetEdges(Direction.Incoming).Count());
        //        Assert.AreEqual(0, nodeb.GetEdges(Direction.Outgoing).Count());

        //        tx.Complete();
        //    }
        //}

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task MultiGraphTest()
        {
            var store = new Store();

            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test1");
            var id1 = new Identity("Test1", "1");

            var dm2 = await store.CreateDomainModelAsync("Test2");
            var id2 = new Identity("Test2", "1");

            using (var session = domain.Store.BeginSession())
            {
                session.Execute(new Hyperstore.Modeling.Commands.AddEntityCommand(domain, PrimitivesSchema.SchemaEntitySchema, id1));
                session.Execute(new Hyperstore.Modeling.Commands.AddEntityCommand(dm2, PrimitivesSchema.SchemaEntitySchema, id2));

                var source = store.GetElement(id1, PrimitivesSchema.SchemaEntitySchema);
                var target = store.GetElement(id2, PrimitivesSchema.SchemaEntitySchema);
                session.Execute(new Hyperstore.Modeling.Commands.AddRelationshipCommand(PrimitivesSchema.ModelRelationshipSchema, source, target, new Identity("Test1", "10")));
                session.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                var src = store.GetElement(id1, PrimitivesSchema.SchemaEntitySchema);
                var end = store.GetElement(id2, PrimitivesSchema.SchemaEntitySchema);

                Assert.AreEqual(1, domain.GetRelationships(start: src).Count());
                Assert.AreEqual(0, dm2.GetRelationships(end: end).Count());

                Assert.IsNotNull(domain.GetRelationships(start: src).FirstOrDefault());
                Assert.AreEqual("10", domain.GetRelationships(start: src).First().Id.Key);
                Assert.AreEqual(id2, domain.GetRelationships(start: src).First().End.Id);
            }
        }

        [TestMethod()]
        [TestCategory("Hypergraph")]
        public async Task TraversalTest()
        {
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test"); 

            CreateGraph(domain);

            ISchemaEntity metadata;
            using (var session = domain.Store.BeginSession(new SessionConfiguration { Readonly = true }))
            {
                metadata = TestDomainDefinition.XExtendsBaseClass;
            }

            // Définition d'un query
            var query = new TraversalQuery(domain)
                {
                    // Filtre personnalisé 
                    // On prend en compte les chemins dont le noeud terminal est le 7 et on ignore les autres
                    EvaluatorFactory = p=>p.EndElement.Id.Key == "7" 
                        ? GraphTraversalEvaluatorResult.IncludeAndContinue 
                        : GraphTraversalEvaluatorResult.ExcludeAndContinue                       
                };

                var cx = query.GetPaths(domain.GetElement(new Identity("test", "1"), metadata)).Count();



                Assert.AreEqual(3, cx);

                query.PathTraverser = new GraphDepthFirstTraverser();
                query.EvaluatorFactory = p => p.EndElement.Id.Key == "7" ? GraphTraversalEvaluatorResult.IncludeAndExit : GraphTraversalEvaluatorResult.ExcludeAndContinue;

                //visitor = new FirstPathVisitor(GraphTestBuilder.GetVertex(Graph, 7));
                var p2 = query.GetPaths(domain.GetElement(new Identity("test", "1"), metadata)).First();
                Assert.AreEqual(2, p2.Length);

            //Assert.IsTrue(visitor.Path != null && visitor.Path.Length == 2);
            //using (var tx = domainModel.Item2.BeginTransaction())
            //{
            //    var edge = GraphTestBuilder.GetEdge(Graph, 10);
            //    Graph.RemoveEdge(edge.Id);
            //    tx.Commit();
            //}

            //visitor = new FirstPathVisitor(GraphTestBuilder.GetVertex(Graph, 7));
            //query.Traverse(visitor);

            //Assert.IsTrue(visitor.Path != null && visitor.Path.Length == 3);

        }
    }
}
