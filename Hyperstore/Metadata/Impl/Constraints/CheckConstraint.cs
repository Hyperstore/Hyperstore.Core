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
    public class CheckConstraint<T> : ICheckConstraint<T> where T : IModelElement
    {
        private readonly Func<T, bool> expression;
        private readonly bool isWarning;
        private readonly string _message;

        public CheckConstraint(string message, Func<T, bool> expression, bool isWarning)
        {
            Contract.RequiresNotEmpty(message, "message");
            Contract.Requires(expression, "expression");

            this._message = message;
            this.expression = expression;
            this.isWarning = isWarning;
        }

        void ICheckConstraint<T>.Check(T mel, ConstraintContext ctx)
        {
            if (!expression(mel))
            {
                if (isWarning)
                {
                    ctx.CreateWarningMessage(_message);
                }
                else
                {
                    ctx.CreateErrorMessage(_message);
                }
            }
        }
    }
}
