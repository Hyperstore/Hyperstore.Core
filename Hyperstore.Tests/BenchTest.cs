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
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using System.Collections.Concurrent;
using Hyperstore.Modeling;
using System.Threading.Tasks;
using Hyperstore.Tests.Model;
using System.Diagnostics;
using Hyperstore.Modeling.Platform;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests
{
    
    public class BenchTest : HyperstoreTestBase
    {
        private ConcurrentDictionary<int, Identity> ids;
        private IHyperstore store;
        private ISchema<TestDomainDefinition> schema;

        [Fact]
        public async Task Bench()
        {
            store = await StoreBuilder.New().CreateAsync();
            schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var sw = new Stopwatch();

            sw.Start();
            var mx = 100;
            AddElement(domain, mx);
            UpdateElement(mx);
            ReadElement(mx);
            RemoveElement(mx);
            sw.Stop();
            Trace.WriteLine("Bench : " + sw.ElapsedMilliseconds.ToString());
            Assert.True(sw.ElapsedMilliseconds < 2000);
        }

        // [Fact]
        public async Task BenchWithConstraints()
        {
            long nb = 0;
            store = await StoreBuilder.New().CreateAsync();
            schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var sw = new Stopwatch();

            // Ajout de 100 contraintes
            var nbc = 100;
            for (int i = 0; i < nbc; i++)
                schema.Definition.XExtendsBaseClass.AddImplicitConstraint(
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

            Assert.Equal(mx * nbc * 2, nb); // Nbre de fois la contrainte est appelée (sur le add et le update)
            Assert.True(sw.ElapsedMilliseconds < 4000, String.Format("ElapsedTime = {0}", sw.ElapsedMilliseconds));
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
                    IModelElement a = store.GetElement(ids[i], schema.Definition.XExtendsBaseClass);
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

            var x = store.GetElements(schema.Definition.XExtendsBaseClass).Count();
            var y = ids.Count();
            Assert.True(x == 0 && y == 0);
            return x + y;
        }
    }
}
