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
using System.Linq;
using System.Text;
using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using Hyperstore.Tests.Model;
using Hyperstore.Modeling.Metadata.Constraints;

namespace Hyperstore.Tests.Model
{
    public partial class Library : ICheckConstraint<Library>
    {
        void ICheckConstraint<Library>.Check(Library mel, ConstraintContext ctx)
        {
         
        }
    }

    public class EmailSchema : Hyperstore.Modeling.Metadata.SchemaValueObject<string>, ICheckValueObjectConstraint<string>
    {
        public EmailSchema(ISchema schema)
            : base(schema)
        {
        }

        protected override object Deserialize(SerializationContext ctx)
        {
            return Hyperstore.Modeling.Metadata.Primitives.StringPrimitive.DeserializeString(ctx);
        }

        protected override string Serialize(object data, IJsonSerializer serializer)
        {
            return Hyperstore.Modeling.Metadata.Primitives.StringPrimitive.SerializeString(data);
        }

        public void Check(string value, ConstraintContext ctx)
        {
            try
            {
                new System.Net.Mail.MailAddress(value);
            }
            catch(Exception)
            {
                ctx.CreateErrorMessage("Invalid email address {Email} for {Name}");
            }
        }
    }
}

namespace Hyperstore.Tests
{
    /// <summary>
    /// Creation d'une classe X dont le name est test
    /// </summary>
    class MyCommand : AbstractDomainCommand, ICommandHandler<MyCommand>
    {
        public XExtendsBaseClass Element { get; private set; }

        public MyCommand(IDomainModel domainModel)
            : base(domainModel)
        {
        }

        public Modeling.Events.IEvent Handle(ExecutionCommandContext<MyCommand> context)
        {
            Element = new XExtendsBaseClass(DomainModel);
            Element.Name = "Test";
            return new MyEvent(DomainModel, context.CurrentSession.SessionId);
        }
    }

    public class MyEvent : Hyperstore.Modeling.Events.AbstractDomainEvent
    {
        public MyEvent(IDomainModel domainModel, Guid correlationId)
            : base(domainModel.Name, domainModel.ExtensionName, 1, correlationId)
        {

        }
    }
}
