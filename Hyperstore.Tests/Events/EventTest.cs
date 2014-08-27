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
using Hyperstore.Modeling.Domain;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
    public class EventTest
    {
        [TestMethod]
        public async Task CreationEvents()
        {
            var store = StoreBuilder.New().Create();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().UsingIdGenerator(r=>new LongIdGenerator()).CreateAsync("Test"); 

            int cx = 0;
            // Abonnements aux events
            domain.Events.CustomEventRaised.Subscribe(e =>
            {
                // Il est le seul en top level
                Assert.IsTrue(e.Event.IsTopLevelEvent);
                cx++;
            });
 
            domain.Events.EntityAdded.Subscribe(e =>
            {
                Assert.IsFalse(e.Event.IsTopLevelEvent);
                cx++;
            });

            domain.Events.PropertyChanged.Subscribe(e =>
            {
                Assert.IsFalse(e.Event.IsTopLevelEvent);
                cx++;
            });

            domain.Events.SessionCompleted.Subscribe(e =>
            {
                Assert.IsFalse(e.IsAborted);
                Assert.IsFalse(e.IsReadOnly);
                cx++;
            });

            domain.Events.PropertyChanged.Subscribe(e =>
            {
                if (e.Event.ElementId == new Identity("Test", "1"))
                {
                    Assert.IsFalse(e.Event.IsTopLevelEvent);
                    cx++;
                }
            });

            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
            }

            Assert.AreEqual(5, cx);
        }

        [TestMethod]
        public async Task DeletionEvents()
        {
            var store = StoreBuilder.New().Create();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test"); 

            int cx = 0;
            // Abonnements aux events
            domain.Events.EntityRemoved.Subscribe(e =>
            {
                Assert.IsTrue(e.Event.IsTopLevelEvent);
                cx++;
            });

            domain.Events.PropertyRemoved.Subscribe(e =>
            {
                Assert.IsFalse(e.Event.IsTopLevelEvent);
                cx++;
            });

            domain.Events.SessionCompleted.Subscribe(e =>
            {
                Assert.IsFalse(e.IsAborted);
                Assert.IsFalse(e.IsReadOnly);
                cx++;
            });

            IModelElement elem;
            using (var s = store.BeginSession())
            {
                var cmd = new MyCommand(domain);
                s.Execute(cmd);
                elem = cmd.Element;
                s.AcceptChanges();
            }

            using (var s = store.BeginSession())
            {
                elem.Remove();
                s.AcceptChanges();
            }

            Assert.AreEqual(4, cx);
        }

    }
}
