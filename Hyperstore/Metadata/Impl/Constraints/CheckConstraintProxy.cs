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
    internal class CheckConstraintProxy : ICheckConstraint
    {
        private readonly Action<IModelElement, ConstraintContext, object> _handler;
        private readonly object _constraint;

        public CheckConstraintProxy(Type implementedType, object constraint)
        {
            _handler = CreateCheckHandler(implementedType);
            _constraint = constraint;
        }

        public void Check(IModelElement mel, ConstraintContext ctx)
        {
            _handler(mel, ctx, _constraint);
        }

        private Action<IModelElement, ConstraintContext, object> CreateCheckHandler(Type elementType)
        {
            DebugContract.Requires(elementType);

            var constraintType = typeof(ICheckConstraint<>).MakeGenericType(elementType);


            // Génération en dynamique d'un appel en utilisant un contexte typé
            var pmel = Expression.Parameter(typeof(IModelElement));
            var pctx = Expression.Parameter(typeof(ConstraintContext));
            var pconstraint = Expression.Parameter(constraintType);

            // (mel, ctx, constraint) => constraint.Check((T)mel, ctx)
            var invocationExpression = Expression.Lambda(
                                        Expression.Block(
                                            Expression.Call(Expression.Convert(pconstraint, constraintType),
                                                        Hyperstore.Modeling.Utils.ReflectionHelper.GetMethod(constraintType, "Check").First(),
                                                            Expression.Convert(pmel, elementType),
                                                            Expression.Constant(pctx))),
                                            pmel, pctx, pconstraint);

            return (Action<IModelElement, ConstraintContext, object>)invocationExpression.Compile();
        }
    }
}
