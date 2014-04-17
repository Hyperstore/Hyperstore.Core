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
