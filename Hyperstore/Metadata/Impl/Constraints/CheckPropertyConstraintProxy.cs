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
    class CheckPropertyConstraintProxy : ICheckConstraint<IModelElement>
    {
        private ISchemaProperty _property;
        private Action<object, ConstraintContext, object> _handler;
        private object _constraint;

        public CheckPropertyConstraintProxy(ISchemaProperty property, Type constraintElementType, object constraint)
        {
            this._property = property;
            this._constraint = constraint;
            this._handler = CreateCheckHandler(constraintElementType);
        }

        public void Check(IModelElement mel, ConstraintContext ctx)
        {
            var pv = mel.GetPropertyValue(_property);
            if (pv == null)
                return;

            ctx.PropertyName = _property.Name;
            _handler(pv.Value, ctx, _constraint);
        }

        private Action<object, ConstraintContext, object> CreateCheckHandler(Type elementType)
        {
            DebugContract.Requires(elementType);

            var constraintType = typeof(ICheckValueObjectConstraint<>).MakeGenericType(elementType);

            // Génération en dynamique d'un appel en utilisant un contexte typé
            var pvalue = Expression.Parameter(typeof(object));
            var pctx = Expression.Parameter(typeof(ConstraintContext));
            var pconstraint = Expression.Parameter(constraintType);

            // (mel, ctx, constraint) => constraint.Check((T)mel, ctx)
            var invocationExpression = Expression.Lambda(
                                        Expression.Block(
                                            Expression.Call(Expression.Convert(pconstraint, constraintType),
                                                        Hyperstore.Modeling.Utils.ReflectionHelper.GetMethod(constraintType, "Check").First(),
                                                            Expression.Convert(pvalue, elementType),
                                                            Expression.Constant(pctx))),
                                            pvalue, pctx, pconstraint);

            return (Action<object, ConstraintContext, object>)invocationExpression.Compile();
        }
    }
}
