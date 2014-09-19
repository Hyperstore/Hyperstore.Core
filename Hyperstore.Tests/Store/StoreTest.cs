//	Copyright ? 2013 - 2014, Alain Metge. All rights reserved.
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
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<TestDomainDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().UsingIdGenerator(r=>new Hyperstore.Modeling.Domain.LongIdGenerator()).CreateAsync("Test");

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
