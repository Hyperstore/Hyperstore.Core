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
