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
            var store = new Store();
            IDomainModel dm = await store.CreateDomainModelAsync("Test");
            Assert.IsTrue((bool)PrimitivesSchema.BooleanSchema.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, PrimitivesSchema.BooleanSchema.Serialize(true))));
            Assert.IsFalse((bool)PrimitivesSchema.BooleanSchema.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, PrimitivesSchema.BooleanSchema.Serialize(false))));
            Assert.IsFalse((bool)PrimitivesSchema.BooleanSchema.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, null)));
            Assert.IsNull(PrimitivesSchema.BooleanSchema.Serialize(null));
        }

        [TestMethod]
        public async Task DateTimeTest()
        {
            var store = new Store();
            var dt = DateTime.Today;
            IDomainModel dm = await store.CreateDomainModelAsync("Test");
            Assert.AreEqual(dt, (DateTime)PrimitivesSchema.DateTimeSchema.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, PrimitivesSchema.DateTimeSchema.Serialize(dt))));
            Assert.IsNull(PrimitivesSchema.DateTimeSchema.Serialize(null));
        }

        [TestMethod]
        public async Task DoubleTest()
        {
            var store = new Store();
            var dt = 10.2;
            IDomainModel dm = await store.CreateDomainModelAsync("Test");
            Assert.AreEqual(dt, (Double)PrimitivesSchema.DoubleSchema.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, PrimitivesSchema.DoubleSchema.Serialize(dt))));
            Assert.IsNull(PrimitivesSchema.DoubleSchema.Serialize(null));
        }

        [TestMethod]
        public async Task TimeSpanTest()
        {
            var store = new Store();
            var dt = DateTime.Now.TimeOfDay;
            IDomainModel dm = await store.CreateDomainModelAsync("Test");
            Assert.AreEqual((TimeSpan)PrimitivesSchema.TimeSpanSchema.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, PrimitivesSchema.TimeSpanSchema.Serialize(dt))), dt);
            Assert.IsNull(PrimitivesSchema.TimeSpanSchema.Serialize(null));
        }

        [TestMethod]
        public async Task FloatTest()
        {
            var store = new Store();
            var dt = 10.2;
            IDomainModel dm = await store.CreateDomainModelAsync("Test");
            Assert.IsTrue((float)PrimitivesSchema.SingleSchema.Deserialize(new SerializationContext(dm, Identity.Empty, PrimitivesSchema.SchemaEntitySchema, PrimitivesSchema.SingleSchema.Serialize(dt))) - dt < 0.01);
            Assert.IsNull(PrimitivesSchema.SingleSchema.Serialize(null));
        }

        private enum X { A, B,C };
        //[TestMethod]
        //public void EnumTest()
        //{
        //    var store = new Store();
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
