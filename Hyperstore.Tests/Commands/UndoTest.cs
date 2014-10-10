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
using Xunit;
using System.Threading.Tasks;
using Hyperstore.Modeling.Domain;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    
    public class UndoTest : HyperstoreTestBase
    {
        [Fact]
        
        public async Task Undo()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New()
                                .UsingIdGenerator(r => new LongIdGenerator())
                                .Using<IHyperstoreTrace>( r=> new DebugHyperstoreTrace(TraceCategory.Hypergraph))
                                .CreateAsync("Test");

            var undoManager = new UndoManager(store);
            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib1";
                for (int i = 0; i < 5; i++)
                {
                    var b = new Book(domain);
                    b.Title = "Book \"book\" " + i.ToString();
                    b.Copies = i + 1;
                    lib.Books.Add(b);

                    var m = new Member(domain);
                    m.Name = "Book " + i.ToString();
                    lib.Members.Add(m);

                }
                session.AcceptChanges();
            }
            undoManager.RegisterDomain(domain);

            using (var s = store.BeginSession())
            {
                s.Execute(new RemoveEntityCommand(lib));
                s.AcceptChanges();
            }

            Assert.Equal(0, store.GetElements().Count());
            undoManager.Undo();

            Assert.Equal(11, store.GetEntities().Count());
            undoManager.Redo();
            Assert.Equal(0, store.GetElements().Count());

        }

        [Fact]
        
        public async Task UndoWithEnum()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();

            var domain = await store.DomainModels.New().CreateAsync("Test");
            var undoManager = new UndoManager(store);
            
            undoManager.RegisterDomain(domain);

            YClass y;
            using (var s = store.BeginSession())
            {
                y = new YClass(domain);
                s.AcceptChanges();
            }

            using (var s = store.BeginSession())
            {
                y.Direction = Model.Direction.West;
                s.AcceptChanges();
            }

            undoManager.Undo();
            Assert.Equal(Model.Direction.South, y.Direction);

            undoManager.Redo();
            Assert.Equal(Model.Direction.West, y.Direction);

        }

        [Fact]
        
        public async Task UndoWithMultipleDelete()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var undoManager = new UndoManager(store);
            undoManager.RegisterDomain(domain);

            XExtendsBaseClass a;
            using (var s = store.BeginSession())
            {
                a = new XExtendsBaseClass(domain);
                a.Name = "mama";
                s.AcceptChanges();
            }

            Assert.True(undoManager.CanUndo);
            Assert.False(undoManager.CanRedo);

            using (var s = store.BeginSession())
            {
                a.Name = "momo";
                s.AcceptChanges();
            }

            Assert.True(undoManager.CanUndo);
            Assert.False(undoManager.CanRedo);

            undoManager.Undo();
            Assert.True(undoManager.CanRedo);
            Assert.True(undoManager.CanUndo);
            Assert.Equal("mama", a.Name);
        }

        [Fact]
        
        public async Task UndoInEmbeddedSessions()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");
            var undoManager = new UndoManager(store);
            undoManager.RegisterDomain(domain);

            XExtendsBaseClass a;
            using (var s = store.BeginSession())
            {
                a = new XExtendsBaseClass(domain);
                a.Name = "mama";
                s.AcceptChanges();
            }

            Assert.True(undoManager.CanUndo);
            Assert.False(undoManager.CanRedo);

            using (var s = store.BeginSession())
            {
                using (var s2 = store.BeginSession())
                {
                    a.Name = "momo";
                    s2.AcceptChanges();
                }
                s.AcceptChanges();
            }

            Assert.True(undoManager.CanUndo);
            Assert.False(undoManager.CanRedo);

            undoManager.Undo();
            Assert.True(undoManager.CanRedo);
            Assert.True(undoManager.CanUndo);
            Assert.Equal("mama", a.Name);
        }
    }
}
