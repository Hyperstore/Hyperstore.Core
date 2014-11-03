using Hyperstore.Modeling;
using Hyperstore.Modeling.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.XTests
{
    class CultureInfoSchema : SchemaValueObject<CultureInfo>
    {
        protected CultureInfoSchema() { }

        public CultureInfoSchema(ISchema schema)
            : base(schema)
        {

        }
        protected override object Serialize(object data)
        {
            var c = data as CultureInfo;
            return c == null ? null : c.DisplayName;
        }

        protected override object Deserialize(SerializationContext ctx)
        {
            if (ctx.Value == null)
                return null;

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
}
