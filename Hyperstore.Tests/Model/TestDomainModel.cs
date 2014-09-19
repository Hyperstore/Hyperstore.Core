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

        protected override string Serialize(object data, IJsonSerializer serializer)
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
        private Action<IServicesContainer> _prepareDependency;

        public TestDomainDefinition(Action<IServicesContainer> handler, DomainBehavior behavior = DomainBehavior.Observable)
            : base("Hyperstore.Tests", behavior)
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

        protected override IServicesContainer PrepareScopedContainer(IServicesContainer defaultDependencyResolver)
        {
            Using<Hyperstore.Modeling.HyperGraph.IIdGenerator>(r => new LongIdGenerator());

            var services = base.PrepareScopedContainer(defaultDependencyResolver);
            if (_prepareDependency != null)
                _prepareDependency(services);
            return services;
        }
    }
}
