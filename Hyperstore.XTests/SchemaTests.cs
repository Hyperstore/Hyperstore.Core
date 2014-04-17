using Hyperstore.Modeling;
using Hyperstore.Modeling.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hyperstore.XTests
{
    public class SchemaTests
    {
        class CultureInfoSchema : SchemaValueObject<CultureInfo>
        {
            public CultureInfoSchema(ISchema schema)
                : base(schema)
            {

            }
            protected override string Serialize(object data)
            {
                if (data == null)
                    return null;
                return ((CultureInfo)data).DisplayName;
            }

            protected override object Deserialize(SerializationContext ctx)
            {
                if (ctx.Value == null)
                    return DefaultValue;
                return new CultureInfo((string)ctx.Value);
            }
        }

        class MySchemaDefinition : SchemaDefinition
        {
            public bool IsSchemaLoaded { get; private set; }

            public MySchemaDefinition()
                : base("Hyperstore.XTests")
            {
            }

            protected override void DefineSchema(ISchema schema)
            {
                new CultureInfoSchema(schema);
            }

            protected override void OnSchemaLoaded(ISchema schema)
            {
                base.OnSchemaLoaded(schema);
                IsSchemaLoaded = true;
            }
        }

        [Fact]
        public void CheckPrimitives()
        {
            var store = new Store();
            Assert.NotNull(store.GetSchemaInfo<string>());
            Assert.NotNull(store.GetSchemaInfo<int>());
            Assert.NotNull(store.GetSchemaInfo<double>());
        }

        [Fact]
        public async void SchemaEvents()
        {
            var store = new Store();
            var def = new MySchemaDefinition();            
            var schema = await store.LoadSchemaAsync(def);
            Assert.True(def.IsSchemaLoaded);
        }

        [Fact]
        public async Task LoadSchema()
        {
            var store = new Store();
            var def = new MySchemaDefinition();
            var schema = await store.LoadSchemaAsync(def);
            Assert.NotNull(schema);
            Assert.Equal(1, schema.GetSchemaInfos().Count());
            Assert.Equal(2, store.Schemas.Count());
        }

        [Fact]
        public async void UnloadSchema()
        {
            var store = new Store();
            var def = new MySchemaDefinition();
            var schema = await store.LoadSchemaAsync(def);
            Assert.NotNull(schema);
            store.UnloadSchemaOrExtension(schema);
            Assert.Equal(1, store.Schemas.Count());
        }

        [Fact]
        public void UnloadPrimitivesSchemaMustFailed()
        {
            var store = new Store();
            var schema = store.Schemas.First();
            Assert.NotNull(schema);
            Assert.IsType<PrimitivesSchema>(schema);
            Assert.Throws<Exception>( ()=> store.UnloadSchemaOrExtension(schema));
        }
    }
}