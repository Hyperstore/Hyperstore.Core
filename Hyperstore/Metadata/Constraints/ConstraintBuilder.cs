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
    ///  Create a constraint builder.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    public class ConstraintBuilder<T> where T : IModelElement
    {
        private ISchemaElement _metadata;
        private Func<T, bool> _expression;
        private string _message;
        private bool _isImplicit;
        private string _category;
        private bool _isWarning;
        private string _propertyName;

        private ConstraintBuilder(ISchemaElement metadata, string propertyName, Func<T, bool> expression, bool isImplicit)
        {
            this._metadata = metadata;
            this._expression = expression;
            this._isImplicit = isImplicit;
            this._propertyName = propertyName;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initialize a new constraint builder. You must call its Register method to create an dregister the constraint
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema element associates to the constraint
        /// </param>
        /// <param name="propertyName">
        ///  Property name if any (Used to personalize the message and to send a data error info if the domain is observable)
        /// </param>
        /// <param name="expression">
        ///  Expression used to validate the constraint
        /// </param>
        /// <param name="message">
        ///  (Optional) Message to display when the constraint is not respected
        /// </param>
        /// <param name="isImplicit">
        ///  (Optional) true if this instance is implicit.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder(ISchemaElement schemaElement, string propertyName, Func<T, bool> expression = null, DiagnosticMessage message = null, bool isImplicit = false)
            : this(schemaElement, propertyName, expression, isImplicit)
        {
            Message(message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initialize a new constraint builder. You must call its Register method to create an dregister the constraint
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema element associates to the constraint
        /// </param>
        /// <param name="propertyName">
        ///  Property name if any (Used to personalize the message and to send a data error info if the domain is observable)
        /// </param>
        /// <param name="expression">
        ///  Expression used to validate the constraint
        /// </param>
        /// <param name="message">
        ///  (Optional) Message to display when the constraint is not respected
        /// </param>
        /// <param name="isImplicit">
        ///  (Optional) true if this instance is implicit.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder(ISchemaElement schemaElement, string propertyName, Func<T, bool> expression, string message = null, bool isImplicit = false)
            : this(schemaElement, propertyName, expression, isImplicit)
        {
            this._message = message;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Configure the constraint from a diagnostic message properties.
        /// </summary>
        /// <param name="message">
        ///  A diagnostic message
        /// </param>
        /// <returns>
        ///  A ConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder<T> Message(DiagnosticMessage message)
        {
            Contract.Requires(message, "message");
            this._message = message.Message;
            this._isWarning = message.MessageType == MessageType.Warning;
            this._category = message.Category;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Expression used to validate the constraint
        /// </summary>
        /// <param name="expression">
        ///  The expression must return true to validate the constraint.
        /// </param>
        /// <returns>
        ///  A ConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder<T> Verify(Func<T, bool> expression)
        {
            Contract.Requires(expression, "expression");
            this._expression = expression;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Define a category used when a validation is called
        /// </summary>
        /// <param name="category">
        ///  The category.
        /// </param>
        /// <returns>
        ///  A ConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder<T> Category(string category)
        {
            this._category = category;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set the message to display when the constraint is not respected
        /// </summary>
        /// <param name="message">
        ///  Text Message : Can contain property pattern like {Name}, {Id}...
        /// </param>
        /// <returns>
        ///  A ConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder<T> Message(string message)
        {
            Contract.RequiresNotEmpty(message, "message");
            this._message = message;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  This constraint will raise a warning message (The session wil not be aborted)
        /// </summary>
        /// <returns>
        ///  A ConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder<T> AsWarning()
        {
            _isWarning = true;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set this constraint as implicit (run on every change)
        /// </summary>
        /// <returns>
        ///  A ConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintBuilder<T> AsImplicit()
        {
            _isImplicit = true;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register this constraint in the constraint manager
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Register()
        {
            var schema = _metadata.Schema;

            if (String.IsNullOrEmpty(_message))
            {
                _message = "Constraint failed for element {Name} ({Id}).";
            }
            else if (!_message.Contains("{Id}"))
            {
                _message = _message.Trim() + " for element {Name} ({Id}).";
            }

            if (_isImplicit)
            {
                var constraint = new CheckConstraint<T>(_message, _expression, _isWarning, _propertyName);
                schema.Constraints.AddConstraint<T>(_metadata, constraint);
            }
            else
            {
                var constraint = new ValidateConstraint<T>(_message, _expression, _isWarning, _category, _propertyName);
                schema.Constraints.AddConstraint<T>(_metadata, constraint);
            }
        }
    }
}
