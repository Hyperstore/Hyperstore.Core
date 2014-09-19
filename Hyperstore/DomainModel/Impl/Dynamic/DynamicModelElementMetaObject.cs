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
 
#region Imports

using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling
{
    /// <summary>
    ///     Implémentation d'un dynamicmetaobject
    /// </summary>
    internal class DynamicModelElementMetaObject : DynamicMetaObject
    {
        internal DynamicModelElementMetaObject(Expression parameter, object value) : base(parameter, BindingRestrictions.Empty, value)
        {
            DebugContract.Requires(parameter, "parameter");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs the binding of the dynamic set member operation.
        /// </summary>
        /// <param name="binder">
        ///  An instance of the <see cref="T:System.Dynamic.SetMemberBinder" /> that represents the
        ///  details of the dynamic operation.
        /// </param>
        /// <param name="value">
        ///  The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the value for the set
        ///  member operation.
        /// </param>
        /// <returns>
        ///  The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the
        ///  binding.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            // Method to call in the containing class:
            const string methodName = "TrySetProperty";

            // setup the binding restrictions.
            var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            // setup the parameters:
            var args = new Expression[2];
            // First parameter is the name of the property to Set
            args[0] = Expression.Constant(binder.Name);
            // Second parameter is the value
            args[1] = Expression.Convert(value.Expression, typeof (object));

            // Setup the 'this' reference
            Expression self = Expression.Convert(Expression, LimitType);

            // Setup the method call expression
            Expression methodCall = Expression.Call(self, ReflectionHelper.GetMethod(typeof (DynamicModelEntity), methodName)
                    .First(), args);

            // Create a meta object to invoke Set later:
            var setDictionaryEntry = new DynamicMetaObject(methodCall, restrictions);

            // return that dynamic object
            return setDictionaryEntry;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs the binding of the dynamic get member operation.
        /// </summary>
        /// <param name="binder">
        ///  An instance of the <see cref="T:System.Dynamic.GetMemberBinder" /> that represents the
        ///  details of the dynamic operation.
        /// </param>
        /// <returns>
        ///  The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the
        ///  binding.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            // Method call in the containing class:
            const string methodName = "TryGetProperty";

            // One parameter
            var parameters = new Expression[] {Expression.Constant(binder.Name)};

            var getDictionaryEntry = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType), ReflectionHelper.GetMethod(typeof (DynamicModelEntity), methodName)
                    .First(), parameters), BindingRestrictions.GetTypeRestriction(Expression, LimitType));

            return getDictionaryEntry;
        }
    }
}