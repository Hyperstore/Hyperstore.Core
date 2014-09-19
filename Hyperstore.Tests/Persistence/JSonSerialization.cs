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
using System.IO;
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.HyperGraph.Index;
using Hyperstore.Modeling.Serialization;
using Hyperstore.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Commands
{
    [TestClass()]
    public class JSonSerializationTest : HyperstoreTestBase
    {
        [TestMethod]
        public async Task SerializeElementAsJson()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New().CreateAsync("Test");

            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib1";
                for(int i=0;i<3;i++)
                {
                    var b  = new Book(domain);
                    b.Title = "Book \"book\" " + i.ToString();
                    b.Copies = i + 1;
                    lib.Books.Add(b);

                    var m = new Member(domain);
                    m.Name = "Book " + i.ToString();
                    lib.Members.Add(m);

                }
                session.AcceptChanges();
            }

            var json = JSonSerializer.Serialize(lib, JSonSerializationOption.Json );
            Assert.IsTrue(!String.IsNullOrEmpty( json) );

            var newton = Newtonsoft.Json.JsonConvert.SerializeObject(lib, new Newtonsoft.Json.JsonSerializerSettings { PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects });
           
            store.DomainModels.Unload(domain);
            domain = await store.DomainModels.New().CreateAsync("Test");
            using (var session = store.BeginSession(new SessionConfiguration { DefaultDomainModel = domain, Mode= SessionMode.Loading }))
            {
                Newtonsoft.Json.JsonConvert.DeserializeObject<Library>(json);
                session.AcceptChanges();
            }
            lib = domain.GetEntities<Library>().FirstOrDefault();
            Assert.IsNotNull(lib);
            Assert.AreEqual(3, lib.Books.Count());
        }
     
    }
}
