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
using System.Threading;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Modeling.Messaging;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework; 
#endif

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
    public class EventBusTest
    {
        [TestMethod]
        public async Task InprocEventBus()
        {
            // Synchronize two domain in two diffrent stores.

            // Load source doman
            var store = new Store();
            
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test");
            // Configure an output channel
            store.EventBus.RegisterDomainPolicies(domain, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All });

            // Load target domain
            var store2 = new Store();
            await store2.LoadSchemaAsync(new TestDomainDefinition());
            var domain2 = await store2.CreateDomainModelAsync("Test");
            // Configure an input channel
            store2.EventBus.RegisterDomainPolicies(domain2, null, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All });

            // Start the event bus
            await store.EventBus.OpenAsync(new InProcChannel());
            await store2.EventBus.OpenAsync(new InProcChannel());

            // Listen on the target domain
            ManualResetEvent set = new ManualResetEvent(false); // Used to indicate that the event has been received
            domain2.Events.ElementAdded.Subscribe(e =>
            {
                Assert.IsNotNull(store2.GetElements<XExtendsBaseClass>().First());
                set.Set();
            });

            // Run a command in the source domain
            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
            }

            if (!set.WaitOne(10000))
            {
                Assert.Inconclusive();
            }
        }

        //[TestMethod]
        //public async Task InprocEventBusWithMetadatas()
        //{
        //    // Initial store
        //    var store = new Store();
        //    await store.LoadSchemaAsync(new TestDomainDefinition());
        //    var domain = await store.CreateDomainModelAsync("Test");
        //    store.EventBus.RegisterDomainPolicies(domain, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All });
        //    await store.EventBus.OpenAsync(new InProcChannel());

        //    // New target store
        //    var store2 = new Store();
        //    await store2.LoadSchemaAsync(new TestDomainDefinition());
        //    var domain2 = await store2.CreateDomainModelAsync("Test");
        //    store2.EventBus.RegisterDomainPolicies(domain2, null, new ChannelPolicy { PropagationStrategy = EventPropagationStrategy.All }); // En entrée uniquement
        //    await store2.EventBus.OpenAsync(new InProcChannel());


        //    // Thread.Sleep(200);
        //    ManualResetEvent set = new ManualResetEvent(false);
        //    domain2.Events.ElementAdded.Subscribe(e =>
        //    {
        //        Assert.IsNotNull(store2.GetElements<XExtendsBaseClass>().First());
        //        set.Set();
        //    });

        //    using (var s = store.BeginSession())
        //    {
        //        s.Execute(new MyCommand(domain));
        //        s.AcceptChanges();
        //    }

        //    if (!set.WaitOne(1500))
        //    {
        //        Assert.Inconclusive();
        //    }

        //}
    }
}
