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
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using System.Collections.Concurrent;
using Hyperstore.Modeling;
using System.Threading.Tasks;
using Hyperstore.Tests.Model;
using System.Diagnostics;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests
{
    [TestClass]
    public class BenchTest : HyperstoreTestBase
    {
        private ConcurrentDictionary<int, Identity> ids;
        private IHyperstore store;

        [TestMethod]
        public async Task Bench()
        {
            store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition("Hyperstore.Tests.Model"));
            var domain = await store.CreateDomainModelAsync("Test");
            var sw = new Stopwatch();

            sw.Start();
            var mx = 100;
            AddElement(domain, mx);
            UpdateElement(mx);
            ReadElement(mx);
            RemoveElement(mx);
            sw.Stop();
            Trace.WriteLine("Bench : " + sw.ElapsedMilliseconds.ToString());
            Assert.IsTrue(sw.ElapsedMilliseconds < 2000);
        }

        [TestMethod]
        public async Task BenchWithConstraints()
        {
            long nb = 0;
            store = new Store();
            await store.LoadSchemaAsync(new TestDomainDefinition("Hyperstore.Tests.Model"));
            var domain = await store.CreateDomainModelAsync("Test");
            var sw = new Stopwatch();

            // Ajout de 100 contraintes
            var nbc = 100;
            for (int i = 0; i < nbc; i++)
                TestDomainDefinition.XExtendsBaseClass.AddImplicitConstraint(
                    self =>
                        System.Threading.Interlocked.Increment(ref nb) > 0,
                    "OK");

            sw.Start();
            var mx = 10000;
            AddElement(domain, mx);
            UpdateElement(mx);
            ReadElement(mx);
            RemoveElement(mx);
            sw.Stop();

            Assert.AreEqual(mx * nbc * 2, nb); // Nbre de fois la contrainte est appelée (sur le add et le update)
            Assert.IsTrue(sw.ElapsedMilliseconds < 4000, String.Format("ElapsedTime = {0}", sw.ElapsedMilliseconds));
        }

        private void AddElement(IDomainModel domain, int max)
        {
            ids = new ConcurrentDictionary<int, Identity>();

            Parallel.For(0, max, i =>
            {
                using (var tx = store.BeginSession())
                {
                    var a = new XExtendsBaseClass(domain);
                    if (ids.TryAdd(i, ((IModelElement)a).Id))
                        tx.AcceptChanges();
                }
            });
        }

        private void UpdateElement(int max)
        {
            Parallel.For(0, max, i =>
            {
                using (var tx = store.BeginSession())
                {
                    XExtendsBaseClass a = store.GetElement<XExtendsBaseClass>(ids[i]);
                    a.Name = "Toto" + i;
                    tx.AcceptChanges();
                }
            });
        }

        private void ReadElement(int max)
        {
            //Parallel.For(0, max, i =>
            for (int i = 0; i < max; i++)
            {
                using (var tx = store.BeginSession())
                {
                    XExtendsBaseClass a = store.GetElement<XExtendsBaseClass>(ids[i]);
                    var x = a.Name;
                    tx.AcceptChanges();
                }
            }
            //);
        }

        private int RemoveElement(int max)
        {
            Parallel.For(0, max, i =>
            {
                using (var tx = store.BeginSession())
                {
                    IModelElement a = store.GetElement(ids[i], TestDomainDefinition.XExtendsBaseClass);
                    //if (a != null)
                    {
                        Identity id;
                        if (!ids.TryRemove(i, out id) || id != ((IModelElement)a).Id)
                            throw new Exception();
                        a.Remove();
                    }
                    tx.AcceptChanges();
                }
            }
            );

            var x = store.GetElements(TestDomainDefinition.XExtendsBaseClass).Count();
            var y = ids.Count();
            Assert.IsTrue(x == 0 && y == 0);
            return x + y;
        }
    }
}
