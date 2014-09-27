//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    internal sealed class ConstraintProxy : AbstractConstraintProxy 
    {
        private Action<object, ConstraintContext, object> CheckHandler { get; set; }

        public ConstraintProxy(Type implementedType, object constraint, ConstraintKind kind, string category)
            : base(constraint, kind, category)
        {

            CheckHandler = CreateCheckHandler(implementedType);
        }

        public override void ExecuteConstraint(IModelElement mel, ConstraintContext ctx)
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

        private Type MakeGenericType(Type elementType)
        {
            return typeof(ICheckConstraint<>).MakeGenericType(elementType);
        }
    }
}
