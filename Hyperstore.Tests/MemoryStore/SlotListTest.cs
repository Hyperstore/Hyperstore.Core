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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using Hyperstore.Modeling;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.MemoryStore
{
    [TestClass]
    public class SlotListTest
    {
        [TestMethod]
        public void AddItem()
        {
            SlotList list = new SlotList(NodeType.Node);
            list.Add(new Slot<int>(1));
            Assert.IsTrue(list.GetSlots().Count() == 1);
        }


        [TestMethod]
        public void Enumerate()
        {
            SlotList list = new SlotList(NodeType.Node);
            list.Add(new Slot<int>(1));
            list.Add(new Slot<int>(1));
            Assert.AreEqual(2, list.GetSlots().Sum(i => ((Slot<int>)i).Value));
        }

#if !NETFX_CORE
        [TestMethod]
        public void Garbage()
        {
            SlotList list = new SlotList(NodeType.Node);
            var a = new Slot<int>(1);
            list.Add(a);
            a = new Slot<int>(1);
            list.Add(a);
            a = new Slot<int>(1);
            list.Add(a);
            list.Dispose();
            AssertHelper.IsGarbageCollected(ref list);

            list = new SlotList(NodeType.Node);
            list.Dispose();
            AssertHelper.IsGarbageCollected(ref list);

            list = new SlotList(NodeType.Node);
            a = new Slot<int>(1);
            list.Add(a);
            list.Dispose();
            AssertHelper.IsGarbageCollected(ref list);
        }
#endif

    }
}
