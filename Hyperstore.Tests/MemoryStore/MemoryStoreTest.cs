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
 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Hyperstore.Modeling;
using System.Diagnostics;
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using Hyperstore.Tests.Model;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Metadata;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.MemoryStore
{
    // http://eric.themoritzfamily.com/understanding-psqls-mvcc.html
    /// <summary>
    ///This is a test class for MvccPersistenceProviderTest and is intended
    ///to contain all MvccPersistenceProviderTest Unit Tests
    ///</summary>
    [TestClass]
    public class MemoryStoreTest : HyperstoreTestBase
    {
        class Data : GraphNode
        {
            public Data(string key, int v)
                : base(new Identity("test", key), PrimitivesSchema.Int32Schema.Id, NodeType.Node, value:v)
            {
            }
        }

        /// <summary>
        /// Deadlock
        ///</summary>
        [TestMethod]
        public async Task DeadlockTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().Set(Setting.MaxTimeBeforeDeadlockInMs, 300).CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var gate = new System.Threading.ManualResetEvent(false);

            var factory = new TaskFactory();

            var t1 = factory.StartNew(() =>
            {
                // Prend la ressource est la garde
                using (var s = store.BeginSession())
                {
                    s.AcquireLock(LockType.Exclusive, "a");
                    gate.Set();
                    Sleep(1000);
                }
            });

            var t2 = factory.StartNew(() =>
            {
                gate.WaitOne();
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.Serializable }))
                {
                    s.AcquireLock(LockType.Exclusive, "a");
                }
            });

            try
            {
                Task.WaitAll(t1, t2);
            }
            catch (AggregateException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(DeadLockException));
            }
        }


        [TestMethod]
        public async Task DeadlockTest2()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().Set(Setting.MaxTimeBeforeDeadlockInMs, 300).CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var gate = new System.Threading.ManualResetEvent(false);

            var factory = new TaskFactory();

            var t1 = factory.StartNew(() =>
            {
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.Serializable }))
                {
                    gate.Set();
                    s.AcquireLock(LockType.Exclusive, "a");
                    Sleep(150);
                }
            });
            Sleep(50);

            var t2 = factory.StartNew(() =>
            {
                gate.WaitOne();
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.Serializable }))
                {
                    // Récupération du lock mais la transaction t1 à fait un rollback
                    // on peu donc continuer sans erreur   
                    s.AcquireLock(LockType.Exclusive, "a");
                    // OK
                }
            });

            Task.WaitAll(t1, t2);
        }

        [TestMethod]
        public async Task SerializeTransactionExceptionTest()
        {
            // Accéder à une même donnée ds une session Serializable génere une SerializeTransactionException
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().Set(Setting.MaxTimeBeforeDeadlockInMs, 100).CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var gate = new System.Threading.ManualResetEvent(false);

            var factory = new TaskFactory();

            var t1 = factory.StartNew(() =>
            {
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.Serializable }))
                {
                    s.AcquireLock(LockType.Exclusive, "a");
                    gate.Set();
                    Sleep(200);
                    s.AcceptChanges();
                }
            });

            var t2 = factory.StartNew(() =>
            {
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.Serializable }))
                {
                    gate.WaitOne();
                    // Récupération du lock mais la transaction t1 à fait un commit
                    // on obtient une SerializeTransactionException
                    Debug.WriteLine("T2 acquire");
                    s.AcquireLock(LockType.Exclusive, "a");
                }
            });

            try
            {
                Task.WaitAll(t1, t2);
                Assert.Inconclusive();
            }
            catch (AggregateException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(SerializableTransactionException));
            }
        }


        [TestMethod]
        public async Task AcquireExclusive2Test()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var factory = new TaskFactory();

            var tasks = new Task[2];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = factory.StartNew(() =>
                {
                    using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
                    {
                        s.AcquireLock(LockType.ExclusiveWait, "a");
                        Sleep(100);

                        s.AcceptChanges();
                    }

                });
            }

            Task.WaitAll(tasks);
#if TEST
            Assert.IsTrue(((LockManager)((IStore)store).LockManager).IsEmpty());
#endif
        }

        [TestMethod]
        public async Task AcquireExclusiveWaitTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var factory = new TaskFactory();

            var created = false;
            var cx = 0;
            var tasks = new Task[100];
            for (var i = 0; i < tasks.Length; i++)
            {
                var x = i;
                tasks[i] = factory.StartNew(() =>
                {
                    using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.Serializable }))
                    {
                        if (x % 2 == 0)
                        {
                            s.AcquireLock(LockType.Shared, "a");

                        }
                        else if (!created)
                        {
                            s.AcquireLock(LockType.ExclusiveWait, "a");

                            if (!created)
                            {
                                created = true;
                                cx++;
                                Sleep(200);
                            }
                        }
                        s.AcceptChanges();
                    }
                });
            }

            Task.WaitAll(tasks);
            Assert.AreEqual(1, cx);
#if TEST
            Assert.IsTrue(((LockManager)((IStore)store).LockManager).IsEmpty());
#endif

        }

        [TestMethod]
        public async Task NestedSession()
        {
            var store = await StoreBuilder.New().CreateAsync();
            store.Services.Register<ITransactionManager>(new MockTransactionManager());
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var provider = new TransactionalMemoryStore(domain);
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                using (var s2 = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
                {
                    provider.AddNode(new Data("1", 10));
                    provider.UpdateNode(new Data("1", 20));
                    s2.AcceptChanges();
                }
                s.AcceptChanges();
            }

            Assert.AreEqual(((Data)provider.GetNode(new Identity("test", "1"))).Value, 20);
        }

        [TestMethod]
        public async Task RollbackNestedSession()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var provider = new TransactionalMemoryStore(domain);
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                using (var s2 = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
                {
                    provider.AddNode(new Data("1", 10));
                    provider.UpdateNode(new Data("1", 20));
                }
            }

            using (var s = store.BeginSession())
            {
                Assert.AreEqual(provider.GetNode(new Identity("test", "1")), null);
                s.AcceptChanges();
            }
        }

        [TestMethod]
        public async Task NestedSessionRollback()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var provider = new TransactionalMemoryStore(domain);
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.AddNode(new Data("1", 10));
                using (var s2 = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
                {
                    provider.UpdateNode(new Data("1", 20));
                }
                s.AcceptChanges();
            }

            using (var s = store.BeginSession())
            {
                Assert.AreEqual(provider.GetNode(new Identity("test", "1")), null);
                s.AcceptChanges();
            }
        }

        [TestMethod]
        public async Task CommitTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var provider = new TransactionalMemoryStore(domain);
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.AddNode(new Data("1", 10));
                s.AcceptChanges();
            }
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                Assert.AreEqual(((Data)provider.GetNode(new Identity("test", "1"))).Value, 10);
                s.AcceptChanges();
            }
        }

        [TestMethod]
        public async Task RollbackTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            var provider = new TransactionalMemoryStore(domain);
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.AddNode(new Data("1", 10));
            }

            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                Assert.AreEqual(provider.GetNode(new Identity("test", "1")), null);
                s.AcceptChanges();
            }
        }

        [TestMethod]
        public async Task RollbackUpdateTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var provider = new TransactionalMemoryStore(domain);
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.AddNode(new Data("1", 10));
                s.AcceptChanges();
            }

            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.UpdateNode(new Data("1", 20));
            }
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                Assert.AreEqual(((Data)provider.GetNode(new Identity("test", "1"))).Value, 10);
                s.AcceptChanges();
            }
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var provider = new TransactionalMemoryStore(domain);
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.AddNode(new Data("1", 10));
                s.AcceptChanges();
            }

            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.RemoveNode(new Identity("test", "1"));
            }

            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                Assert.AreEqual(((Data)provider.GetNode(new Identity("test", "1"))).Value, 10);
            }

            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                provider.RemoveNode(new Identity("test", "1"));
                s.AcceptChanges();
            }

            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                Assert.AreEqual(provider.GetNode(new Identity("test", "1")), null);
                s.AcceptChanges();
            }
        }

        [TestMethod]
        public async Task SerializableTransactionTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            Identity aid;
            dynamic a;

            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                a = new DynamicModelEntity(domain, TestDomainDefinition.XExtendsBaseClass);
                aid = a.Id;
                a.Value = 10;
                s.AcceptChanges();
            }

            var factory = new TaskFactory();
            var signal = new System.Threading.ManualResetEventSlim();
            var signal2 = new System.Threading.ManualResetEventSlim();

            var t1 = factory.StartNew(() =>
            {
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.Serializable }))
                {
                    // Accès sur A pour ouvrir une transaction Serializable (la session ne fait rien). Ce n'est que qu'en on 
                    // accède à la donnée que la transaction est crèèe
                    var x = a.Value; // Ou n'importe quoi 
                    signal2.Set();
                    signal.Wait();
                    Assert.AreEqual(a.Value, 10); // On ne "voit" pas les modifications des autres transactions
                }
            });

            Sleep(50);

            var t2 = factory.StartNew(() =>
            {
                signal2.Wait();
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
                {
                    a.Value = 11;
                    s.AcceptChanges();
                }
                signal.Set();
            });

            Task.WaitAll(t1, t2);
        }

        [TestMethod]
        public async Task ReadPhantomTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            Identity aid;
            dynamic a;

            // Création d'une valeur
            using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
            {
                a = new DynamicModelEntity(domain, TestDomainDefinition.XExtendsBaseClass);
                aid = a.Id;
                a.Value = 10;
                s.AcceptChanges();
            }

            var factory = new TaskFactory();
            var signal = new System.Threading.ManualResetEventSlim();
            var signal2 = new System.Threading.ManualResetEventSlim();

            var t1 = factory.StartNew(() =>
            {
                // Cette transaction démarre avant l'autre mais elle va pouvoir 'voir' ses modifs commités 
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
                {
                    signal2.Set();
                    signal.Wait();// On attend le commit de l'autre                    
                    Assert.AreEqual(11, a.Value); // On "voit" dèja que la valeur a été modifiée
                }
            });

            Sleep(50);

            var t2 = factory.StartNew(() =>
            {
                signal2.Wait(); // On s'assure de démarrer aprés l'autre transaction
                using (var s = store.BeginSession(new SessionConfiguration { IsolationLevel = SessionIsolationLevel.ReadCommitted }))
                {
                    a.Value = 11;
                    s.AcceptChanges();
                }
                signal.Set();
            });

            Task.WaitAll(t1, t2);

        }
    }
}
