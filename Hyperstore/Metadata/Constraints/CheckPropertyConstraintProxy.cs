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
