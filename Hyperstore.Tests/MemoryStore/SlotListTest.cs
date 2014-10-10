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
using Xunit;
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using Hyperstore.Modeling;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.MemoryStore
{
    
    public class SlotListTest
    {
        [Fact]
        public void AddItem()
        {
            SlotList list = new SlotList(null, NodeType.Node);
            list.Add(new Slot<int>(1));
            Assert.True(list.Length == 1);
        }


        [Fact]
        public void Enumerate()
        {
            SlotList list = new SlotList(null, NodeType.Node);
            list.Add(new Slot<int>(1));
            list.Add(new Slot<int>(1));
            Assert.Equal(2, list.Sum(i => ((Slot<int>)i).Value));
        }

#if !NETFX_CORE
        [Fact]
        public void Garbage()
        {
            SlotList list = new SlotList(null, NodeType.Node);
            var a = new Slot<int>(1);
            list.Add(a);
            a = new Slot<int>(1);
            list.Add(a);
            a = new Slot<int>(1);
            list.Add(a);
            list.Dispose();
            AssertHelper.IsGarbageCollected(ref list);

            list = new SlotList(null, NodeType.Node);
            list.Dispose();
            AssertHelper.IsGarbageCollected(ref list);

            list = new SlotList(null, NodeType.Node);
            a = new Slot<int>(1);
            list.Add(a);
            list.Dispose();
            AssertHelper.IsGarbageCollected(ref list);
        }
#endif

    }
}
