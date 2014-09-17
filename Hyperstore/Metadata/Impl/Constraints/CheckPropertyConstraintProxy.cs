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
using System.Linq.Expressions;
using System.Text;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    internal sealed class CheckPropertyConstraintProxy : ConstraintProxy
    {
        private ISchemaProperty _property;

        public CheckPropertyConstraintProxy(ISchemaProperty property, Type constraintElementType, object constraint, ConstraintKind kind, string category)
            : base(constraintElementType, constraint, kind, category)
        {
            this._property = property;
        }

        public override void ExecuteConstraint(IModelElement mel, ConstraintContext ctx)
        {
            var pv = mel.GetPropertyValue(_property);
            if (pv == null)
                return;

            ctx.PropertyName = _property.Name;
            CheckHandler(pv.Value, ctx, Constraint);
        }

        protected override Type MakeGenericType(Type elementType)
        {
            return typeof(ICheckValueObjectConstraint<>).MakeGenericType(elementType);
        }
    }
}
