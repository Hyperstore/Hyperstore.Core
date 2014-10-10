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
using Xunit;
using Hyperstore.Modeling;
using Hyperstore.Modeling.MemoryStore;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.MemoryStore
{
    
    public class MemoryCommandContextTest : Hyperstore.Tests.HyperstoreTestBase 
    {
        [Fact]
        public async Task Create_CommandContext_inside_a_current_transaction()
        {
            var store = await StoreBuilder.New().CreateAsync();
            using (var session = store.BeginSession())
            {
                TransactionManager tm = new Hyperstore.Tests.MemoryStore.MockTransactionManager();
                using (var tx = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                {
                    var c = new CommandContext(tm);
                    Assert.Equal(c.CommandId, 0);
                    Assert.Equal(c.Transaction, tx);
                    c.Dispose();
                }
            }
        }

        [Fact]
        public async Task CommandId_must_be_incremented()
        {
            var store = await StoreBuilder.New().CreateAsync();
            using (var session = store.BeginSession())
            {
                TransactionManager tm = new Hyperstore.Tests.MemoryStore.MockTransactionManager();
                using (var tx = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                {
                    var c = new CommandContext(tm);
                    Assert.Equal(c.CommandId, 0);
                    c.Dispose();
                    c = new CommandContext(tm);
                    Assert.Equal(c.CommandId, 1);
                    c.Dispose();
                }
            }
        }

        [Fact]
        public void IsTransactionValid()
        {            
            TransactionManager tm = new Hyperstore.Tests.MemoryStore.MockTransactionManager();
            using (var tx1 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
            {
                var c = new CommandContext(tm);
                Assert.True(c.IsTransactionValid(tx1.Id));
                using (var tx2 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                {
                    Assert.True(c.IsTransactionValid(tx2.Id));
                    tx2.Commit();
                    tx1.Commit();
                    Assert.True(c.IsTransactionValid(tx1.Id));
                    using (var tx3 = tm.BeginTransaction(SessionIsolationLevel.ReadCommitted))
                    {
                        Assert.True(c.IsTransactionValid(tx1.Id));
                        Assert.True(c.IsTransactionValid(tx3.Id));
                    }
                }
            }
        }

        //[Fact]
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
        //            Assert.False(c.IsTransactionValid(tx2.Id));
        //            Assert.False(c.IsTransactionValid(tx1.Id));
        //        }
        //    }
        //}

        //[Fact]
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
        //        Assert.True(c.IsValidInSnapshot(slot));
        //        c.Dispose();
        //        // Nouvelle commande
        //        c = new CommandContext(tm);
        //        // Simule la suppression
        //        slot = new StubISlot() { XMinGet = () => 1, CMinGet = () => 0, XMaxGet = () => 2 };
        //        Assert.False(c.IsValidInSnapshot(slot));
        //        c.Dispose();
        //    }
        //}
    }
}
