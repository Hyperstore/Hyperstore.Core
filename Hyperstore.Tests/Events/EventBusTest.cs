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
using System.Threading;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Modeling.Messaging;
using Hyperstore.Tests.Model;
using Xunit;
using System.Threading.Tasks;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework; 
#endif

namespace Hyperstore.Tests.Commands
{
    
    public class EventBusTest
    {
        [Fact]
        public async Task InprocEventBus()
        {
            // Synchronize two domain in two diffrent stores.

            // Load source doman
            var store = await StoreBuilder.New().CreateAsync();
            
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            // Configure an output channel
            store.EventBus.RegisterDomainPolicies(domain, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All });

            // Load target domain
            var store2 = await StoreBuilder.New().CreateAsync();
            await store2.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain2 = await store2.DomainModels.New().CreateAsync("Test");
            // Configure an input channel
            store2.EventBus.RegisterDomainPolicies(domain2, null, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All });

            // Start the event bus
            await store.EventBus.OpenAsync(new InProcChannel());
            await store2.EventBus.OpenAsync(new InProcChannel());

            // Listen on the target domain
            ManualResetEvent set = new ManualResetEvent(false); // Used to indicate that the event has been received
            domain2.Events.EntityAdded.Subscribe(e =>
            {
                Assert.NotNull(store2.GetElements<XExtendsBaseClass>().First());
                set.Set();
            });

            // Run a command in the source domain
            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
            }

            if (!set.WaitOne(1000))
            {
                throw new Exception("Inconclusive");
            }
        }

        //[Fact]
        //public async Task InprocEventBusWithMetadatas()
        //{
        //    // Initial store
        //    var store = await StoreBuilder.Init().CreateStore();
        //    await store.Schemas.New<TestDomainDefinition>().CreateAsync();
        //    var domain = await store.DomainModels.New().CreateAsync("Test");
        //    store.EventBus.RegisterDomainPolicies(domain, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All });
        //    await store.EventBus.OpenAsync(new InProcChannel());

        //    // New target store
        //    var store2 = await StoreBuilder.Init().CreateStore();
        //    await store2.LoadSchemaAsync(new TestDomainDefinition());
        //    var domain2 = await store2.CreateDomainModelAsync("Test");
        //    store2.EventBus.RegisterDomainPolicies(domain2, null, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All }); // En entrée uniquement
        //    await store2.EventBus.OpenAsync(new InProcChannel());


        //    // Thread.Sleep(200);
        //    ManualResetEvent set = new ManualResetEvent(false);
        //    domain2.Events.ElementAdded.Subscribe(e =>
        //    {
        //        Assert.NotNull(store2.GetElements<XExtendsBaseClass>().First());
        //        set.Set();
        //    });

        //    using (var s = store.BeginSession())
        //    {
        //        s.Execute(new MyCommand(domain));
        //        s.AcceptChanges();
        //    }

        //    if (!set.WaitOne(1500))
        //    {
        //                        throw new Exception("Inconclusive");
        //    }

        //}
    }
}
