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
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
    public class CommandEventTest
    {
        class MyCommand : ICommandHandler<MyCommand>, IDomainCommand
        {
            public MyCommand(IDomainModel domainModel)
            {
                DomainModel = domainModel;
            }

            public IDomainModel DomainModel
            {
                get;
                private set;
            }

            public Modeling.Events.IEvent Handle(ExecutionCommandContext<MyCommand> context)
            {
                var a = new XExtendsBaseClass(DomainModel);
                a.Name = "Test";
                return new MyEvent(DomainModel, context.CurrentSession.SessionId);
            }
        }

        public class MyEvent : Hyperstore.Modeling.Events.AbstractDomainEvent
        {
            public MyEvent(IDomainModel domainModel, int correlationId)
                : base(domainModel.Name, domainModel.ExtensionName, 1, correlationId)
            {

            }
        }

        [TestMethod]
        public async Task EventLevel()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test"); 
            var undoManager = new UndoManager(store);
            undoManager.RegisterDomain(domain);

            XExtendsBaseClass a;
            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
                a = store.GetEntities<XExtendsBaseClass>().FirstOrDefault(x => x.Name == "Test");
            }

            Assert.IsTrue(undoManager.CanUndo);
            Assert.IsFalse(undoManager.CanRedo);

            undoManager.Undo();
            Assert.IsTrue(undoManager.CanRedo);
            Assert.IsFalse(undoManager.CanUndo);

            AssertHelper.ThrowsException<Exception>(() =>
            {
                using (var s = store.BeginSession())
                {
                    // a is invalid
                    a.Name = "momo";
                    s.AcceptChanges();
                }
            }
            );
        }
    }
}
