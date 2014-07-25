using System;
using System.Globalization;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Metadata.Primitives;
using Hyperstore.Tests.Model;


namespace Hyperstore.Tests.Model
{
    public class CultureInfoSchema : SchemaValueObject<CultureInfo>
    {
        public CultureInfoSchema(ISchema metaModel)
            : base(metaModel)
        {
        }

        protected override string Serialize(object data)
        {
            var c = data as CultureInfo;
            return c == null ? null : c.DisplayName;
        }

        protected override object Deserialize(SerializationContext ctx)
        {
            if (ctx.Value == null)
                return null;
            if (ctx.Value is CultureInfo)
                return (CultureInfo)ctx.Value;

            return new CultureInfo((string)ctx.Value);
        }
    }

    public enum Direction
    {
        South,
        North,
        West,
        Est
    }

    public partial class TestDomainDefinition
    {
        private Action<IDependencyResolver> _prepareDependency;

        public TestDomainDefinition()
            : this((Action<IDependencyResolver>)null)
        {
            UsesIdGenerator(r => new LongIdGenerator());
        }

        public TestDomainDefinition(Action<IDependencyResolver> handler = null)
            : base("Hyperstore.Tests", DomainBehavior.Observable)
        {
            _prepareDependency = handler;
        }

        //    public override void DefineMetaModel(IMetaModel metaModel)
        //    {
        //        new EnumPrimitive<Direction>(metaModel);
        //        new CultureInfoMetadata(metaModel);

        //        AbstractClass = new MetaClass<AbstractBaseClass>(metaModel);
        //        AbstractClass.DefineProperty<string>("Name");

        //        XExtendsBaseClass = new MetaClass<XExtendsBaseClass>(metaModel, AbstractClass);
        //        XExtendsBaseClass.DefineProperty<int>("Value");

        //        YClass = new MetaClass<YClass>(metaModel);
        //        YClass.DefineProperty<int>("Value");
        //        YClass.DefineProperty<string>("Name");
        //        YClass.DefineProperty<Direction>("Direction", Direction.South);
        //        YClass.DefineProperty<CultureInfo>("Culture", new CultureInfo("fr"));

        //        XReferencesY = new MetaRelationship<XReferencesY>(XExtendsBaseClass, YClass);
        //        XReferencesX = new MetaRelationship("XReferencesX", XExtendsBaseClass, XExtendsBaseClass, Cardinality.OneToMany );

        //        XExtendsBaseClass.DefineProperty("YRelation", XReferencesY);
        //        XExtendsBaseClass.DefineProperty("OthersX", XReferencesX);
        //    }

        //protected override IDependencyResolver PrepareDependencyResolver(IDependencyResolver parentResolver)
        //{
        //    var resolver = base.PrepareDependencyResolver(parentResolver);
        //    if (_prepareDependency != null)
        //        _prepareDependency(resolver);
        //    resolver.Register<global::Hyperstore.Modeling.ISynchronizationContext>(new global::Hyperstore.Modeling.Utils.UIDispatcher());
        //    return resolver;
        //}
    }
}
