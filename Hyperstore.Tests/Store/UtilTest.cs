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
 
using Xunit;
using System;
using System.Threading.Tasks;
using Hyperstore.Modeling;
using System.Diagnostics;
using Hyperstore.Modeling.MemoryStore;
using System.Linq;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Metadata;
using System.Globalization;
using Hyperstore.Tests.Model;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif


namespace Hyperstore.Tests
{  
    
    public class UtilTest 
    {
        [Fact]
        public void UtilValidateNameTest()
        {
            Conventions.CheckValidName("a_b$");
            Conventions.CheckValidName("a_b$.a10", true);

            Assert.Throws<InvalidNameException>(
                () =>
                Conventions.CheckValidName("a_b$.")
            );

            Assert.Throws<InvalidNameException>(
                ()=>
                Conventions.CheckValidName("a_b$:.")            
                );

            Assert.Throws<InvalidNameException>(
                ()=>
                Conventions.CheckValidName("a_b$.:", true)              
                );
        }

    }
}
