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
using System.Threading.Tasks;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hyperstore.Modeling.HyperGraph;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests
{
    public abstract class HyperstoreTestBase
    {
        public TestContext TestContext
        {
            get
            ;
            set
            ;
        }

        // 1 -> 2 -> 4 -> 6 -> 7
        //        -> 5
        //        -> 3      -> 7
        //   -> 3           -> 7
        protected void CreateGraph(IDomainModel domain, Action settings = null)
        {
            var graph = domain.Resolve<IHyperGraph>();
            var ids = new Identity[8];

            var mid = TestDomainDefinition.XExtendsBaseClass;
            using (var session = domain.Store.BeginSession())
            {
                for (int i = 1; i <= 7; i++)
                {
                    ids[i] = Id(i);
                    graph.CreateEntity(ids[i], mid);
                }

                var cx = 8;
                var rid = TestDomainDefinition.XReferencesX;
                graph.CreateRelationship(Id(cx++), rid, ids[1], mid, ids[2], mid);
                graph.CreateRelationship(Id(cx++), rid, ids[1], mid, ids[3], mid);
                graph.CreateRelationship(Id(cx++), rid, ids[2], mid, ids[4], mid);
                graph.CreateRelationship(Id(cx++), rid, ids[2], mid, ids[5], mid);
                graph.CreateRelationship(Id(cx++), rid, ids[2], mid, ids[3], mid);
                graph.CreateRelationship(Id(cx++), rid, ids[4], mid, ids[6], mid);
                graph.CreateRelationship(Id(cx++), rid, ids[6], mid, ids[7], mid);
                graph.CreateRelationship(Id(cx++), rid, ids[3], mid, ids[7], mid);
                session.AcceptChanges();
            }
        }

        protected Identity Id(int nb)
        {
            return new Identity("Test", nb.ToString());
        }

        private ManualResetEvent gate1;
        private ManualResetEvent gate2;
        private Task[] _tasks;

        protected void StartThread1(Action action)
        {
            gate1 = new ManualResetEvent(false);
            gate2 = new ManualResetEvent(false);
            _tasks = new Task[2];
            _tasks[0] = Task.Factory.StartNew(action);
        }

        protected void StartThread2(Action action)
        {
            _tasks[1] = Task.Factory.StartNew(action);
        }

        protected void WaitThreads()
        {
            Task.WaitAll(_tasks);
        }

        protected void WaitThread1()
        {
            gate1.WaitOne();
        }

        protected void WaitThread2()
        {
            gate2.WaitOne();
        }

        protected void ReleaseThread2()
        {
            gate1.Set();
        }

        protected void ReleaseThread1()
        {
            gate2.Set();
        }

        protected void Sleep(int ms)
        {
#if NETFX_CORE
            Task.Delay(ms);
#else
            Thread.Sleep(ms);
#endif
        }
    }
}
