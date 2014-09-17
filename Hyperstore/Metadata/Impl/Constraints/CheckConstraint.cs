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
using System.Text;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A check constraint.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.Constraints.ICheckConstraint{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public class CheckConstraint<T> : ICheckConstraint<T> where T : IModelElement
    {
        private readonly Func<T, bool> expression;
        private readonly bool isWarning;
        private readonly string _message;
        private readonly string _propertyName;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        /// <param name="expression">
        ///  The expression.
        /// </param>
        /// <param name="isWarning">
        ///  true if this instance is warning.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CheckConstraint(string message, Func<T, bool> expression, bool isWarning, string propertyName=null)
        {
            Contract.RequiresNotEmpty(message, "message");
            Contract.Requires(expression, "expression");

            this._message = message;
            this.expression = expression;
            this.isWarning = isWarning;
            this._propertyName = propertyName;
        }

        void ICheckConstraint<T>.ExecuteConstraint(T mel, ConstraintContext ctx)
        {
            if (!expression(mel))
            {
                if (isWarning)
                {
                    ctx.CreateWarningMessage(_message, _propertyName);
                }
                else
                {
                    ctx.CreateErrorMessage(_message, _propertyName);
                }
            }
        }
    }
}
