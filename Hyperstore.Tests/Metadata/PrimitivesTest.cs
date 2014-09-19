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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Metadata.Primitives;
using Hyperstore.Modeling.Utils;
using System.Linq;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Hyperstore.Tests.Metadata
{
    [TestClass]
    public class PrimitivesTest
    {
        [TestMethod]
        public async Task BooleanTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            IDomainModel dm = await store.DomainModels.New().CreateAsync("Test");
            Assert.IsTrue((bool)PrimitivesSchema.BooleanSchema.Deserialize(new SerializationContext(PrimitivesSchema.BooleanSchema, PrimitivesSchema.BooleanSchema.Serialize(true))));
            Assert.IsFalse((bool)PrimitivesSchema.BooleanSchema.Deserialize(new SerializationContext(PrimitivesSchema.BooleanSchema, PrimitivesSchema.BooleanSchema.Serialize(false))));
            Assert.IsFalse((bool)PrimitivesSchema.BooleanSchema.Deserialize(new SerializationContext(PrimitivesSchema.BooleanSchema, null)));
            Assert.IsNull(PrimitivesSchema.BooleanSchema.Serialize(null));
        }

        [TestMethod]
        public async Task DateTimeTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var dt = DateTime.Today;
            IDomainModel dm = await store.DomainModels.New().CreateAsync("Test");
            Assert.AreEqual(dt, (DateTime)PrimitivesSchema.DateTimeSchema.Deserialize(new SerializationContext(PrimitivesSchema.DateTimeSchema, PrimitivesSchema.DateTimeSchema.Serialize(dt))));
            Assert.IsNull(PrimitivesSchema.DateTimeSchema.Serialize(null));
        }

        [TestMethod]
        public async Task DoubleTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var dt = 10.2;
            IDomainModel dm = await store.DomainModels.New().CreateAsync("Test");
            Assert.AreEqual(dt, (Double)PrimitivesSchema.DoubleSchema.Deserialize(new SerializationContext( PrimitivesSchema.DoubleSchema, PrimitivesSchema.DoubleSchema.Serialize(dt))));
            Assert.IsNull(PrimitivesSchema.DoubleSchema.Serialize(null));
        }

        [TestMethod]
        public async Task TimeSpanTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var dt = DateTime.Now.TimeOfDay;
            IDomainModel dm = await store.DomainModels.New().CreateAsync("Test");
            Assert.AreEqual((TimeSpan)PrimitivesSchema.TimeSpanSchema.Deserialize(new SerializationContext( PrimitivesSchema.TimeSpanSchema, PrimitivesSchema.TimeSpanSchema.Serialize(dt))), dt);
            Assert.IsNull(PrimitivesSchema.TimeSpanSchema.Serialize(null));
        }

        [TestMethod]
        public async Task FloatTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            var dt = 10.2;
            IDomainModel dm = await store.DomainModels.New().CreateAsync("Test");
            Assert.IsTrue((float)PrimitivesSchema.SingleSchema.Deserialize(new SerializationContext(PrimitivesSchema.SingleSchema, PrimitivesSchema.SingleSchema.Serialize(dt))) - dt < 0.01);
            Assert.IsNull(PrimitivesSchema.SingleSchema.Serialize(null));
        }

        private enum X { A, B,C };
        //[TestMethod]
        //public void EnumTest()
        //{
        //    var store = await StoreBuilder.Init().CreateStore();
        //    IDomainModel dm = store.GetDomainModels(ModelType.Model).First();
        //    using (var s = store.BeginSession())
        //    {
        //        ISerializableModelElement ser = new EnumPrimitive<X>(dm.Schema);
        //        // TODO accès aux méthodes privees
        //        Assert.AreEqual(X.B, (X)ser.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, ser.Serialize(X.B))));
        //        Assert.AreEqual(X.B | X.C, (X)ser.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, ser.Serialize(X.C | X.B))));
        //        Assert.IsNull(PrimitivesSchema.SingleSchema.Serialize(null));
        //        s.AcceptChanges();
        //    }
        //}

        [TestMethod]
        public void DefaultValue()
        {
            Assert.AreEqual((double)0, ReflectionHelper.GetDefaultValue(typeof(double)));
            Assert.AreEqual(0, ReflectionHelper.GetDefaultValue(typeof(int)));
            var x = ReflectionHelper.GetDefaultValue(typeof(Nullable<int>));
            Assert.AreEqual((Nullable<int>)null, ReflectionHelper.GetDefaultValue(typeof(Nullable<int>)));
            Assert.AreEqual(X.A, ReflectionHelper.GetDefaultValue(typeof(X)));
            Assert.AreEqual(default(DateTime), ReflectionHelper.GetDefaultValue(typeof(DateTime)));
            Assert.AreEqual((uint)0, ReflectionHelper.GetDefaultValue(typeof(uint)));
            Assert.AreEqual((byte)0, ReflectionHelper.GetDefaultValue(typeof(byte)));
            Assert.AreEqual(false, ReflectionHelper.GetDefaultValue(typeof(bool)));
        }
    }
}
