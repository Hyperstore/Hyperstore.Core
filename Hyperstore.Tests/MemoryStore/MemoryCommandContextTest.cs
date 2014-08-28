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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hyperstore.Modeling;
using Hyperstore.Modeling.MemoryStore;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.MemoryStore
{
    [TestClass]
    public class MemoryCommandContextTest : Hyperstore.Tests.HyperstoreTestBase 
    {
        [TestMethod]
        public async Task Create_CommandContext_inside_a_current_transaction()
        {
            var store = await StoreBuilder.New().CreateAsync();
            using (var session = store.BeginSession())
            {
                TransactionManager tm = new Hyperstore.Tests.MemoryStore.MockTransactionManager();
                using (var tx = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                {
                    var c = new CommandContext(tm);
                    Assert.AreEqual(c.CommandId, 0);
                    Assert.AreEqual(c.Transaction, tx);
                    c.Dispose();
                }
            }
        }

        [TestMethod]
        public async Task CommandId_must_be_incremented()
        {
            var store = await StoreBuilder.New().CreateAsync();
            using (var session = store.BeginSession())
            {
                TransactionManager tm = new Hyperstore.Tests.MemoryStore.MockTransactionManager();
                using (var tx = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                {
                    var c = new CommandContext(tm);
                    Assert.AreEqual(c.CommandId, 0);
                    c.Dispose();
                    c = new CommandContext(tm);
                    Assert.AreEqual(c.CommandId, 1);
                    c.Dispose();
                }
            }
        }

        [TestMethod]
        public void IsTransactionValid()
        {            
            TransactionManager tm = new Hyperstore.Tests.MemoryStore.MockTransactionManager();
            using (var tx1 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
            {
                var c = new CommandContext(tm);
                Assert.IsTrue(c.IsTransactionValid(tx1.Id));
                using (var tx2 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                {
                    Assert.IsTrue(c.IsTransactionValid(tx2.Id));
                    tx2.Commit();
                    tx1.Commit();
                    Assert.IsTrue(c.IsTransactionValid(tx1.Id));
                    using (var tx3 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                    {
                        Assert.IsTrue(c.IsTransactionValid(tx1.Id));
                        Assert.IsTrue(c.IsTransactionValid(tx3.Id));
                    }
                }
            }
        }

        //[TestMethod]
        //public void IsTransactionInvalid()
        //{
        //    TransactionManager tm = new Hyperstore.Tests.MemoryStore.MockTransactionManager();         
        //    using (var tx1 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
        //    {
        //        var c = new CommandContext(tm);
        //        using (var tx2 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
        //        {
        //            tx2.Commit();

        //            c = new CommandContext(tm, SessionIsolationLevel.ReadCommitted);
        //            Assert.IsFalse(c.IsTransactionValid(tx2.Id));
        //            Assert.IsFalse(c.IsTransactionValid(tx1.Id));
        //        }
        //    }
        //}

        //[TestMethod]
        //public void IsValidInSnapshot()
        //{
        //    TransactionManager tm = new TransactionManager(null);
        //    using (var tx1 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
        //    {
        //        using (var c0 = new CommandContext(tm)) { c0.Complete(); } // On simule la commande de création CommandId=0

        //        // Nouvelle commande
        //        var c = new CommandContext(tm);
        //        // Stub créé dans la transaction 1
        //        var slot = new StubISlot() { XMinGet = () => 1, CMinGet = () => 0 };
        //        Assert.IsTrue(c.IsValidInSnapshot(slot));
        //        c.Dispose();
        //        // Nouvelle commande
        //        c = new CommandContext(tm);
        //        // Simule la suppression
        //        slot = new StubISlot() { XMinGet = () => 1, CMinGet = () => 0, XMaxGet = () => 2 };
        //        Assert.IsFalse(c.IsValidInSnapshot(slot));
        //        c.Dispose();
        //    }
        //}
    }
}
