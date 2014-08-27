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
 
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif


namespace Hyperstore.Tests
{
    [TestClass]
    public class StoreTest
    {
        [TestMethod]
        public async Task DisposedTest()
        {
            var store = StoreBuilder.New().Create();
            await store.LoadSchemaAsync(new TestDomainDefinition());
            var domain = await store.CreateDomainModelAsync("Test", new DomainConfiguration().UsingIdGenerator(r=>new Hyperstore.Modeling.Domain.LongIdGenerator()));

            XReferencesY rel = null;

            using (var session = store.BeginSession())
            {
                var lockX = session.AcquireLock(LockType.Shared, "X");
                // Upgrade lock type to Exclusive
                lockX = session.AcquireLock(LockType.Exclusive, "X");

                var lockY = session.AcquireLock(LockType.Exclusive, "Y");

                lockX.Dispose(); // Release lockX
            } // Release lockY

            using (var s = store.BeginSession())
            {
                var start = new XExtendsBaseClass(domain);
                var start2 = new XExtendsBaseClass(domain);
                start.OthersX.Add(start);
                var end = new YClass(domain);
                rel = new XReferencesY(start, end);
                s.AcceptChanges();
            }

            Assert.IsNotNull(rel);
        }

    }
}
