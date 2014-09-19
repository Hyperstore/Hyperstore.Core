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
