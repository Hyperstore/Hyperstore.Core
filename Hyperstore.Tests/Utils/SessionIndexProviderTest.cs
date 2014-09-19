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
 
using Hyperstore.Modeling.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Tests.Utils
{
    [TestClass]
    public class SessionIndexProviderTest
    {
        [TestMethod]
        public void GetFreeIndex()
        {
            var m = new SessionIndexProvider();

            for (int i = 1; i < 32 * 8; i++)
            {
                m.Set(i+1);
            }

            Assert.AreEqual(1, m.GetFirstFreeValue());
            Assert.AreEqual(8 * 32 + 1, m.GetFirstFreeValue());
            m.ReleaseValue(47);
            Assert.AreEqual(47, m.GetFirstFreeValue());
            Assert.AreEqual(8 * 32 + 2, m.GetFirstFreeValue());
        }
    }
}
