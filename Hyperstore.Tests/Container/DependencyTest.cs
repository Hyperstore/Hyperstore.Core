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
using Hyperstore.Modeling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Hyperstore.Modeling.Ioc;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Container
{
    interface IService { int Id { get; } }
    
    class Service : IService, IDisposable {
        public static int Sequence;
        public static int DisposeCount;

        public Service()
        {
            Sequence++;
            Id = Sequence;
        }

        public int Id { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
    
    [TestClass]
    public class DependencyTest
    {
        [TestInitialize]
        public void ClearTest()
        {
            Service.Sequence = Service.DisposeCount = 0;
        }

        [TestMethod]
        public void CreateDefault()
        {
            var store = new Store();
            IDependencyResolverInternal r = new DefaultDependencyResolver();
            r.SetStore(store);

            Assert.IsNotNull(r.Resolve<IHyperstore>());
            r.Dispose();
        }

        [TestMethod]
        public void RegisterSingleton()
        {
            var store = new Store();
            IDependencyResolverInternal r = new DefaultDependencyResolver();
            r.SetStore(store);
            r.Register<IService>(new Service() );

            Assert.AreEqual(1, r.Resolve<IService>().Id);
            Assert.AreEqual(1, r.Resolve<IService>().Id);
            Assert.AreEqual(1, r.Resolve<IService>().Id);
            Assert.AreEqual(1, r.Resolve<IService>().Id);

            r = r.CreateDependencyResolver() as IDependencyResolverInternal;
            Assert.AreEqual(1, r.Resolve<IService>().Id);
            Assert.AreEqual(1, r.Resolve<IService>().Id);

            Assert.AreEqual(1, r.Resolve<IService>().Id);
            Assert.AreEqual(1, r.Resolve<IService>().Id);
            r.Dispose();
        }

        [TestMethod]
        public void RegisterByDomain()
        {
            var store = new Store();
            IDependencyResolverInternal r1 = new DefaultDependencyResolver();
            r1.SetStore(store);
            r1.Register<IService>(resolver => new Service());

            Assert.AreEqual(1, r1.Resolve<IService>().Id);
            Assert.AreEqual(1, r1.Resolve<IService>().Id);
            Assert.AreEqual(1, r1.Resolve<IService>().Id);
            Assert.AreEqual(1, r1.Resolve<IService>().Id);

            var r2 = r1.CreateDependencyResolver();
            Assert.AreEqual(2, r2.Resolve<IService>().Id);
            Assert.AreEqual(2, r2.Resolve<IService>().Id);

            Assert.AreEqual(2, r2.Resolve<IService>().Id);
            Assert.AreEqual(2, r2.Resolve<IService>().Id);

            var r3 = r1.CreateDependencyResolver();
            Assert.AreEqual(3, r3.Resolve<IService>().Id);
            Assert.AreEqual(3, r3.Resolve<IService>().Id);

            Assert.AreEqual(3, r3.Resolve<IService>().Id);
            Assert.AreEqual(3, r3.Resolve<IService>().Id);
            r3.Dispose();
            r2.Dispose();
            r1.Dispose();
        }

        [TestMethod]
        public void RegisterByDomainAndModel()
        {
            var store = new Store();
            IDependencyResolverInternal r1 = new DefaultDependencyResolver();
            r1.SetStore(store);
            r1.Register<IService>(new Service());

            Assert.AreEqual(1, r1.Resolve<IService>().Id);

            var r2 = r1.CreateDependencyResolver();
            r2.Register<IService>(new Service());

            Assert.AreEqual(2, r2.Resolve<IService>().Id);
            Assert.AreEqual(1, r1.Resolve<IService>().Id);

            r2.Dispose();
            r1.Dispose();
        }

        [TestMethod]
        public void RegisterMany()
        {
            var store = new Store();
            IDependencyResolverInternal r1 = new DefaultDependencyResolver();
            r1.SetStore(store);
            r1.Register<IService>(new Service());
            r1.Register<IService>(new Service());

            Assert.AreEqual(2, r1.ResolveAll<IService>().Count());
            Assert.AreEqual(2, r1.Resolve<IService>().Id);

            Assert.AreEqual(1, r1.ResolveAll<IService>().First().Id);
            Assert.AreEqual(2, r1.ResolveAll<IService>().Last().Id);

            var r2 = r1.CreateDependencyResolver();
            r2.Register<IService>(new Service() );

            Assert.AreEqual(3, r2.ResolveAll<IService>().Count());
            Assert.AreEqual(3, r2.Resolve<IService>().Id);

            Assert.AreEqual(3, r2.ResolveAll<IService>().First().Id); // D'abord ceux de r2
            Assert.AreEqual(2, r2.ResolveAll<IService>().Last().Id);  // puis ceux de r1
            r2.Dispose();
            r1.Dispose();
        }

        [TestMethod]
        public void ErrorRegisterAfterACallToResolve()
        {
            AssertHelper.ThrowsException<Exception>(() =>
            {
                var store = new Store();
                IDependencyResolverInternal r1 = new DefaultDependencyResolver();
                r1.SetStore(store);
                r1.Register<IService>(new Service());
                r1.Register<IService>(new Service());

                Assert.AreEqual(2, r1.ResolveAll<IService>().Count());
                r1.Register<IService>(new Service());

                r1.Dispose();
            });
        }

        [TestMethod]
        public void DisposeAll()
        {
            var store = new Store();
            IDependencyResolverInternal r1 = new DefaultDependencyResolver();
            r1.SetStore(store);
            r1.Register<IService>(new Service());
            r1.Register<IService>(new Service());

            var r2 = r1.CreateDependencyResolver();
            r2.Register<IService>(new Service());

            r2.Dispose();
            r1.Dispose();
            Assert.AreEqual(Service.Sequence, Service.DisposeCount);
        }

        [TestMethod]
        public void Settings()
        {
            var store = new Store();
            IDependencyResolverInternal r1 = new DefaultDependencyResolver();
            r1.SetStore(store);
            r1.Register<Setting>(new Setting("X", 1));
            r1.Register<Setting>(new Setting("Y", 1));
            Assert.AreEqual(1, r1.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.AreEqual(1, r1.ResolveAll<Setting>().Where(s=>s.Name=="Y").First().Value);
            
            var r2 = r1.CreateDependencyResolver();
            r2.Register<Setting>(new Setting("X", 2));

            Assert.AreEqual(2, r2.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.AreEqual(1, r2.ResolveAll<Setting>().Where(s => s.Name == "Y").First().Value);

            r2.Dispose();
            r1.Dispose();
        }

        [TestMethod]
        public async Task WithDomains()
        {
            var store = new Store();
            store.DependencyResolver.Register<Setting>(new Setting("X", 1));
            store.DependencyResolver.Register<Setting>(new Setting("Y", 1));

            Assert.AreEqual(1, store.DependencyResolver.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.AreEqual(1, store.DependencyResolver.ResolveAll<Setting>().Where(s => s.Name == "Y").First().Value);

            var schema = await store.LoadSchemaAsync(new Hyperstore.Tests.Model.TestDomainDefinition(
                resolver => resolver.Register<Setting>(new Setting("X", 2))
                ));
            Assert.AreEqual(2, schema.DependencyResolver.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.AreEqual(1, schema.DependencyResolver.ResolveAll<Setting>().Where(s => s.Name == "Y").First().Value);

            store.Dispose();
        }
    }
}
