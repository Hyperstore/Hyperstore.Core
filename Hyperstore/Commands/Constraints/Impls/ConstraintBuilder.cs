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

#endregion

namespace Hyperstore.Modeling.Validations
{
    // RuleFor<T>.CheckProperty<string>(e=>e.Name).Required().LengthGreaterThan(5).Custom(v=>v=="qq");
    // RuleFor<T>.CheckProperty(e=>e.Name).If(v=>v.startsWith("a").LengthGreaterThan(5).Custom(v=>v=="qq");
    // RuleFor<T>.CheckProperty(e=>e.Name).If(v=>v.startsWith("a")).LengthGreaterThan(5).Custom(v=>v=="qq");
    internal class ConstraintBuilder<T> : IConstraintBuilder<T> where T : IModelElement
    {
        private readonly ConstraintsManager.ConstraintProxy<T> _proxy;
        private readonly string _propertyName;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="proxy">
        ///  The proxy.
        /// </param>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder(ConstraintsManager.ConstraintProxy<T> proxy, string propertyName)
        {
            DebugContract.Requires(proxy);
            _proxy = proxy;
            _propertyName = propertyName;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Categories.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="policyName">
        ///  Name of the policy.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<T> Category(string policyName)
        {
            Contract.RequiresNotEmpty(policyName, "policyName");
            if (_proxy.Category == ConstraintsCategory.ImplicitPolicy)
                throw new Exception(ExceptionMessages.CannotSetCategoryOnImplicitConstraint);

            _proxy.Category = policyName;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the implicit.
        /// </summary>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<T> Implicit()
        {
            _proxy.Category = ConstraintsCategory.ImplicitPolicy;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Checks.
        /// </summary>
        /// <param name="expression">
        ///  The expression.
        /// </param>
        /// <param name="message">
        ///  (Optional) the message.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<T> Check(Func<T, bool> expression, string message = null)
        {
            Contract.Requires(expression, "expression");

            _proxy.SetConstraint(new CustomConstraint<T>(expression, message, _propertyName));
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Checks.
        /// </summary>
        /// <param name="expression">
        ///  The expression.
        /// </param>
        /// <param name="message">
        ///  The message.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<T> Check(Func<T, bool> expression, DiagnosticMessage message)
        {
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");

            _proxy.SetConstraint(new CustomConstraint<T>(expression, message));
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Checks the given custom.
        /// </summary>
        /// <param name="custom">
        ///  The custom.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<T> Check(IConstraint<T> custom)
        {
            Contract.Requires(custom, "custom");
            _proxy.SetConstraint(custom);
            return this;
        }
    }
}