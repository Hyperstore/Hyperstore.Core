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
        protected CultureInfoSchema()
        {

        }

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

        protected override object DefaultValue
        {
            get { return CultureInfo.CurrentCulture; }
            set { }
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

        public TestDomainDefinition() : this((Action<IDependencyResolver>)null)
        {
            UsesIdGenerator(r=>new LongIdGenerator());
        }

        public TestDomainDefinition(Action<IDependencyResolver> handler=null) 
            : base("Hyperstore.Tests")
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

        protected override IDependencyResolver PrepareDependencyResolver(IDependencyResolver defaultDependencyResolver)
        {
            var resolver = base.PrepareDependencyResolver(defaultDependencyResolver);
            if (_prepareDependency != null)
                _prepareDependency(resolver);
            return resolver;
        }
    }
}
