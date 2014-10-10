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
using Hyperstore.Modeling;
using Xunit;
using System.Linq;
using Hyperstore.Modeling.Container;
using System.Threading.Tasks;
using Hyperstore.Tests.Model;
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
    
    
    public class DependencyTest
    {
        public DependencyTest()
        {
            Service.Sequence = Service.DisposeCount = 0;
        }

        [Fact]
        public async Task CreateDefault()
        {
            var store = await StoreBuilder.New().CreateAsync();

            Assert.NotNull(store.Services.Resolve<IHyperstore>());
            store.Dispose();
        }

        [Fact]
        public async Task RegisterSingleton()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var r = store.Services;
            r.Register<IService>(new Service() );

            Assert.Equal(1, r.Resolve<IService>().Id);
            Assert.Equal(1, r.Resolve<IService>().Id);
            Assert.Equal(1, r.Resolve<IService>().Id);
            Assert.Equal(1, r.Resolve<IService>().Id);

            r = r.NewScope();
            Assert.Equal(1, r.Resolve<IService>().Id);
            Assert.Equal(1, r.Resolve<IService>().Id);

            Assert.Equal(1, r.Resolve<IService>().Id);
            Assert.Equal(1, r.Resolve<IService>().Id);
            r.Dispose();
        }

        [Fact]
        public async Task HideServiceInScope()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var r = store.Services.NewScope();
            r.Register<IService>(new Service());

            Assert.Equal(1, r.Resolve<IService>().Id);

            r = r.NewScope();
            r.Register<IService>(null);

            Assert.Null(r.Resolve<IService>());
            r.Dispose();
        }

        [Fact]
        public async Task RegisterByDomain()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var r1 = store.Services;
            r1.Register<IService>(services => new Service());

            Assert.Equal(1, r1.Resolve<IService>().Id);
            Assert.Equal(1, r1.Resolve<IService>().Id);
            Assert.Equal(1, r1.Resolve<IService>().Id);
            Assert.Equal(1, r1.Resolve<IService>().Id);

            var r2 = r1.NewScope();
            Assert.Equal(2, r2.Resolve<IService>().Id);
            Assert.Equal(2, r2.Resolve<IService>().Id);

            Assert.Equal(2, r2.Resolve<IService>().Id);
            Assert.Equal(2, r2.Resolve<IService>().Id);

            var r3 = r1.NewScope();
            Assert.Equal(3, r3.Resolve<IService>().Id);
            Assert.Equal(3, r3.Resolve<IService>().Id);

            Assert.Equal(3, r3.Resolve<IService>().Id);
            Assert.Equal(3, r3.Resolve<IService>().Id);
            r3.Dispose();
            r2.Dispose();
            r1.Dispose();
        }

        [Fact]
        public async Task RegisterByDomainAndModel()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var r1 = store.Services;
            r1.Register<IService>(new Service());

            Assert.Equal(1, r1.Resolve<IService>().Id);

            var r2 = r1.NewScope();
            r2.Register<IService>(new Service());

            Assert.Equal(2, r2.Resolve<IService>().Id);
            Assert.Equal(1, r1.Resolve<IService>().Id);

            r2.Dispose();
            r1.Dispose();
        }

        [Fact]
        public async Task RegisterMany()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var r1 = store.Services;
            r1.Register<IService>(new Service());
            r1.Register<IService>(new Service());

            Assert.Equal(2, r1.ResolveAll<IService>().Count());
            Assert.Equal(2, r1.Resolve<IService>().Id);

            Assert.Equal(2, r1.ResolveAll<IService>().First().Id);
            Assert.Equal(1, r1.ResolveAll<IService>().Last().Id);

            var r2 = r1.NewScope();
            r2.Register<IService>(new Service() );

            Assert.Equal(3, r2.ResolveAll<IService>().Count());
            Assert.Equal(3, r2.Resolve<IService>().Id);

            Assert.Equal(3, r2.ResolveAll<IService>().First().Id); // D'abord ceux de r2
            Assert.Equal(1, r2.ResolveAll<IService>().Last().Id);  // puis ceux de r1
            r2.Dispose();
            r1.Dispose();
        }

        [Fact]
        public async Task DisposeAll()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var r1 = store.Services.NewScope();

            r1.Register<IService>(r => new Service());
            r1.Register<IService>(r => new Service());

            var r2 = r1.NewScope();
            r2.Register<IService>(r => new Service());

            r2.ResolveAll<IService>().ToList();

            r2.Dispose();
            r1.Dispose();
            Assert.Equal(Service.Sequence, Service.DisposeCount);
        }

        [Fact]
        public async Task Settings()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var r1 = store.Services;

            r1.Register<Setting>(new Setting("X", 1));
            r1.Register<Setting>(new Setting("Y", 1));
            Assert.Equal(1, r1.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.Equal(1, r1.ResolveAll<Setting>().Where(s=>s.Name=="Y").First().Value);

            var r2 = r1.NewScope();
            r2.Register<Setting>(new Setting("X", 2));

            Assert.Equal(2, r2.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.Equal(1, r2.ResolveAll<Setting>().Where(s => s.Name == "Y").First().Value);

            r2.Dispose();
            r1.Dispose();
        }

        [Fact]
        public async Task WithDomains()
        {
            var store = await StoreBuilder.New().CreateAsync();
            store.Services.Register<Setting>(new Setting("X", 1));
            store.Services.Register<Setting>(new Setting("Y", 1));

            Assert.Equal(1, store.Services.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.Equal(1, store.Services.ResolveAll<Setting>().Where(s => s.Name == "Y").First().Value);

            var schema = await store.Schemas.New<TestDomainDefinition>().Set("X", 2).CreateAsync();
            Assert.Equal(2, schema.Services.ResolveAll<Setting>().Where(s => s.Name == "X").First().Value);
            Assert.Equal(1, schema.Services.ResolveAll<Setting>().Where(s => s.Name == "Y").First().Value);

            store.Dispose();
        }
    }
}
