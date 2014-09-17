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
    public class ConstraintBuilder<T> where T : IModelElement
    {
        private ISchemaElement metadata;
        private Func<T, bool> expression;
        private string _message;
        private bool isImplicit;
        private string category;
        private bool isWarning;

        private ConstraintBuilder(ISchemaElement metadata, string propertyName, Func<T, bool> expression, bool isImplicit)
        {
            this.metadata = metadata;
            this.expression = expression;
            this.isImplicit = isImplicit;
        }

        public ConstraintBuilder(ISchemaElement metadata, string propertyName, Func<T, bool> expression = null, DiagnosticMessage message = null, bool isImplicit = false)
            : this(metadata, propertyName, expression, isImplicit)
        {
            Message(message);
        }

        public ConstraintBuilder(ISchemaElement metadata, string propertyName, Func<T, bool> expression, string message = null, bool isImplicit = false)
            : this(metadata, propertyName, expression, isImplicit)
        {
            this._message = message;
        }

        public ConstraintBuilder<T> Message(DiagnosticMessage message)
        {
            Contract.Requires(message, "message");
            this._message = message.Message;
            this.isWarning = message.MessageType == MessageType.Warning;
            this.category = message.Category;
            return this;
        }

        public ConstraintBuilder<T> Verify(Func<T, bool> expression)
        {
            Contract.Requires(expression, "expression");
            this.expression = expression;
            return this;
        }

        public ConstraintBuilder<T> Category(string category)
        {
            this.category = category;
            return this;
        }

        public ConstraintBuilder<T> Message(string message)
        {
            Contract.RequiresNotEmpty(message, "message");
            this._message = message;
            return this;
        }

        public ConstraintBuilder<T> AsWarning()
        {
            isImplicit = true;
            return this;
        }

        public ConstraintBuilder<T> AsImplicit()
        {
            isWarning = true;
            return this;
        }


        public void Create()
        {
            var schema = metadata.Schema;

            if (String.IsNullOrEmpty(_message))
            {
                _message = "Constraint failed for element {Name} ({Id}).";
            }
            else if (!_message.Contains("{Id}"))
            {
                _message = _message.Trim() + " for element {Name} ({Id}).";
            }

            if (isImplicit)
            {
                var constraint = new CheckConstraint<T>(_message, expression, isWarning);
                schema.Constraints.AddConstraint<T>(metadata, constraint);
            }
            else
            {
                var constraint = new ValidateConstraint<T>(_message, expression, isWarning, category);
                schema.Constraints.AddConstraint<T>(metadata, constraint);
            }
        }
    }
}
