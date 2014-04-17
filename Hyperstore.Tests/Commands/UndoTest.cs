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
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
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
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
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
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
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
            var store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
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
