//	Copyright � 2013 - 2014, Alain Metge. All rights reserved.
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
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
    public class UndoTest : HyperstoreTestBase
    {
        [TestMethod]
        [TestCategory("Commands")]
        public async Task Undo()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var undoManager = new UndoManager(store);
            CreateGraph(domain);
            undoManager.RegisterDomain(domain);

            IModelElement a = store.GetElement<XExtendsBaseClass>(Id(1));
            using (var s = store.BeginSession())
            {
                a.Remove(); // Supprime tout le graphe
                s.AcceptChanges();
            }

            Assert.AreEqual(0, store.GetElements().Count());
            undoManager.Undo();

            Assert.AreEqual(7, store.GetEntities().Count());
            undoManager.Redo();
            Assert.AreEqual(0, store.GetElements().Count());

        }

        [TestMethod]
        [TestCategory("Commands")]
        public async Task UndoWithEnum()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();

            var domain = await store.DomainModels.New().CreateAsync("Test");
            var undoManager = new UndoManager(store);
            
            undoManager.RegisterDomain(domain);

            YClass y;
            using (var s = store.BeginSession())
            {
                y = new YClass(domain);
                s.AcceptChanges();
            }

            using (var s = store.BeginSession())
            {
                y.Direction = Model.Direction.West;
                s.AcceptChanges();
            }

            undoManager.Undo();
            Assert.AreEqual(Model.Direction.South, y.Direction);

            undoManager.Redo();
            Assert.AreEqual(Model.Direction.West, y.Direction);

        }

        [TestMethod]
        [TestCategory("Commands")]
        public async Task UndoWithMultipleDelete()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var undoManager = new UndoManager(store);
            undoManager.RegisterDomain(domain);

            XExtendsBaseClass a;
            using (var s = store.BeginSession())
            {
                a = new XExtendsBaseClass(domain);
                a.Name = "mama";
                s.AcceptChanges();
            }

            Assert.IsTrue(undoManager.CanUndo);
            Assert.IsFalse(undoManager.CanRedo);

            using (var s = store.BeginSession())
            {
                a.Name = "momo";
                s.AcceptChanges();
            }

            Assert.IsTrue(undoManager.CanUndo);
            Assert.IsFalse(undoManager.CanRedo);

            undoManager.Undo();
            Assert.IsTrue(undoManager.CanRedo);
            Assert.IsTrue(undoManager.CanUndo);
            Assert.AreEqual("mama", a.Name);
        }

        [TestMethod]
        [TestCategory("Commands")]
        public async Task UndoInEmbeddedSessions()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var undoManager = new UndoManager(store);
            undoManager.RegisterDomain(domain);

            XExtendsBaseClass a;
            using (var s = store.BeginSession())
            {
                a = new XExtendsBaseClass(domain);
                a.Name = "mama";
                s.AcceptChanges();
            }

            Assert.IsTrue(undoManager.CanUndo);
            Assert.IsFalse(undoManager.CanRedo);

            using (var s = store.BeginSession())
            {
                using (var s2 = store.BeginSession())
                {
                    a.Name = "momo";
                    s2.AcceptChanges();
                }
                s.AcceptChanges();
            }

            Assert.IsTrue(undoManager.CanUndo);
            Assert.IsFalse(undoManager.CanRedo);

            undoManager.Undo();
            Assert.IsTrue(undoManager.CanRedo);
            Assert.IsTrue(undoManager.CanUndo);
            Assert.AreEqual("mama", a.Name);
        }
    }
}
