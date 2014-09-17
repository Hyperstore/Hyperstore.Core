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
    internal class ConstraintProxy : ICheckConstraint
    {
        protected Action<object, ConstraintContext, object> CheckHandler { get; private set; }
        protected object Constraint { get; private set; }

        public ConstraintKind Kind { get; private set; }
        public string Category { get; private set; }

        public ConstraintProxy(Type implementedType, object constraint, ConstraintKind kind, string category)
        {

            CheckHandler = CreateCheckHandler(implementedType);

            Constraint = constraint;
            Kind = kind;
            Category = category;
        }

        public virtual void ExecuteConstraint(IModelElement mel, ConstraintContext ctx)
        {
            CheckHandler(mel, ctx, Constraint);
        }

        private Action<object, ConstraintContext, object> CreateCheckHandler(Type elementType)
        {
            DebugContract.Requires(elementType);

            var constraintType = MakeGenericType(elementType);


            var pmel = Expression.Parameter(typeof(object));
            var pctx = Expression.Parameter(typeof(ConstraintContext));
            var pconstraint = Expression.Parameter(typeof(object));

            // (mel, ctx, constraint) => constraint.Check((T)mel, ctx)
            var invocationExpression = Expression.Lambda(
                                            Expression.Call(Expression.Convert(pconstraint, constraintType),
                                                        Hyperstore.Modeling.Utils.ReflectionHelper.GetMethod(constraintType, "ExecuteConstraint").First(),
                                                            Expression.Convert(pmel, elementType),
                                                            pctx),
                                            pmel, pctx, pconstraint);

            return (Action<object, ConstraintContext, object>)invocationExpression.Compile();
        }

        protected virtual Type MakeGenericType(Type elementType)
        {
            return typeof(ICheckConstraint<>).MakeGenericType(elementType);
        }
    }
}
