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
            public MyEvent(IDomainModel domainModel, Guid correlationId)
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

            AssertHelper.ThrowsException<SessionException>(() =>
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
