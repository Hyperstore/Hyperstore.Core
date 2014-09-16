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
    internal sealed class ValidationConstraintProxy : IValidationConstraint
    {
        private readonly Action<IModelElement, ConstraintContext, object, string> _handler;
        private readonly object _constraint;
        public ValidationConstraintProxy(Type implementedType, object constraint, string category)
        {
            _handler = CreateCheckHandler(implementedType);
            _constraint = constraint;
            Category = category;
        }

        public void Validate(IModelElement mel, ConstraintContext ctx)
        {
            _handler(mel, ctx, _constraint, Category );
        }

        public string Category
        {
            get;
            private set;
        }

        private Action<IModelElement, ConstraintContext, object, string> CreateCheckHandler(Type elementType)
        {
            DebugContract.Requires(elementType);

            var constraintType = typeof(IValidationConstraint<>).MakeGenericType(elementType);


            // Génération en dynamique d'un appel en utilisant un contexte typé
            var pmel = Expression.Parameter(typeof(IModelElement));
            var pctx = Expression.Parameter(typeof(ConstraintContext));
            var pconstraint = Expression.Parameter(constraintType);
            var pcat = Expression.Parameter(typeof(string));

            // (mel, ctx, constraint) => constraint.Check((T)mel, ctx)
            var invocationExpression = Expression.Lambda(
                                        Expression.Block(
                                            Expression.Call(Expression.Convert(pconstraint, constraintType),
                                                            Hyperstore.Modeling.Utils.ReflectionHelper.GetMethod(constraintType, "Validate").First(),
                                                            Expression.Convert(pmel, elementType),
                                                            Expression.Constant(pctx),
                                                            Expression.Constant(pcat))),
                                            pmel, pctx, pconstraint, pcat);

            return (Action<IModelElement, ConstraintContext, object, string>)invocationExpression.Compile();
        }
    }
}
