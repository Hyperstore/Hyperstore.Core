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

            var json = JSonDomainModelSerializer.Serialize(lib, JSonSerializationOption.Json );
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
