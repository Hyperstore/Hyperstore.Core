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