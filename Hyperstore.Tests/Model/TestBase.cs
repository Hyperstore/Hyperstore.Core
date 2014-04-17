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
