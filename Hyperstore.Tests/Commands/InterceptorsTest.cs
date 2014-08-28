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
    public class InterceptorsTest
    {
        class MyInterceptor : ICommandInterceptor<MyCommand>
        {
            private Action _action;
            public MyInterceptor(Action action)
            {
                _action  = action;
            }

            public BeforeContinuationStatus OnBeforeExecution( ExecutionCommandContext<MyCommand> context )
            {
                _action();
                return BeforeContinuationStatus.Continue;
            }

            public ContinuationStatus OnAfterExecution( ExecutionCommandContext<MyCommand> context )
            {
                return ContinuationStatus.Continue;
            }

            public ErrorContinuationStatus OnError( ExecutionCommandContext<MyCommand> context, Exception exception )
            {
                throw new NotImplementedException();
            }

            public bool IsApplicableOn(ISession session, MyCommand command )
            {
                return true;
            }
        }

        class SharedInterceptor : ICommandInterceptor<AbstractDomainCommand>
        {
            private Action _action;
            public SharedInterceptor(Action action)
            {
                _action = action;
            }

            public BeforeContinuationStatus OnBeforeExecution(ExecutionCommandContext<AbstractDomainCommand> context)
            {
                _action();
                return BeforeContinuationStatus.Continue;
            }

            public ContinuationStatus OnAfterExecution(ExecutionCommandContext<AbstractDomainCommand> context)
            {
                return ContinuationStatus.Continue;
            }

            public ErrorContinuationStatus OnError(ExecutionCommandContext<AbstractDomainCommand> context, Exception exception)
            {
                throw new NotImplementedException();
            }

            public bool IsApplicableOn(ISession session, AbstractDomainCommand command)
            {
                return true;
            }
        }

        [TestMethod]
        public async Task InterceptorOrdersEvents()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test"); 

            int cx=0;

            domain.Commands.RegisterInterceptor( new MyInterceptor( () => { cx = 1; } ), 2 );
            domain.Commands.RegisterInterceptor(new MyInterceptor(() => { cx = 2; }), 1);

            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
            }

            Assert.AreEqual(2, cx);
        }

        [TestMethod]
        public async Task SharedInterceptorTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test"); 

            int cx = 0;

            domain.Commands.RegisterInterceptor(new SharedInterceptor(() => { cx = 2; }), 2);

            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
            }

            Assert.AreEqual(2, cx);
        }

        [TestMethod]
        public async Task AddInterceptorTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test"); 

            int cx = 0;

            domain.Commands.RegisterInterceptor(new SharedInterceptor(() => { cx = 1; }), 1);

            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
            }
            Assert.AreEqual(1, cx);
            domain.Commands.RegisterInterceptor(new MyInterceptor(() => { cx = 2; }));
            
            using (var s = store.BeginSession())
            {
                s.Execute(new MyCommand(domain));
                s.AcceptChanges();
            }

            Assert.AreEqual(2, cx);
        }
    }
}
