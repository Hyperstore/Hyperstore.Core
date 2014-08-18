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
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Define a custom constraint based on a simple expression. If you want define a more
    ///  sophisticatic constraint, create a new class implementing IConstraint&gt;T&lt;
    /// </summary>
    /// <typeparam name="T">
    ///  Type of the element to validate
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.Validations.IConstraint{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public class CustomConstraint<T> : IConstraint<T> 
    {
        private readonly Func<T, bool> _expression;
        private readonly DiagnosticMessage _validationMessage;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Custom contraint with an expression and a predefined diagnostic message.
        /// </summary>
        /// <param name="expression">
        ///  Expression to validate. If the expression returns false, a message will be emit.
        /// </param>
        /// <param name="message">
        ///  Predefined message. This message can contains named items like {propertyName} wich will be
        ///  replace by the current specified property value. You can use predefined property Id,
        ///  DomainModel and SchemaInfo and all element defined properties.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CustomConstraint(Func<T, bool> expression, DiagnosticMessage message)
        {
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            _expression = expression;
            _validationMessage = message;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Custom contraint with an expression and a predefined diagnostic message.
        /// </summary>
        /// <param name="expression">
        ///  Expression to validate. If the expression returns false, a message will be emit.
        /// </param>
        /// <param name="message">
        ///  (Optional) Predefined message. This message can contains named items like {propertyName} wich
        ///  will be replace by the current specified property value. You can use predefined property Id,
        ///  DomainModel and SchemaInfo and all element defined properties.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CustomConstraint(Func<T, bool> expression, string message = null, string propertyName=null)
        {
            Contract.Requires(expression, "expression");

            _expression = expression;            
            _validationMessage = new DiagnosticMessage(MessageType.Error, message, "Validation", null, null, propertyName);
        }

        #region IConstraint<T> Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        /// <param name="context">
        ///  The context.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Apply(T element, ISessionContext context)
        {
            Contract.Requires(element != null, "element");
            Contract.Requires(context, "context");

            CodeMarker.Mark(GetType().Name + ".Validate");
            if (!_expression(element))
                context.Log(CreateValidationMessage(element));
        }

        #endregion

        private DiagnosticMessage CreateValidationMessage(T value)
        {
            var message = _validationMessage.Message;

            if (String.IsNullOrEmpty(message))
            {
                message = "Constraint failed for element {Name} ({Id}).";
            }
            else if (!message.Contains("{Id}"))
            {
                message = message.Trim() + " for element {Name} ({Id}).";
            }
    
            return new DiagnosticMessage(_validationMessage.MessageType, MessageHelper.CreateMessage(message, value as IModelElement), _validationMessage.Category ?? "Validation", true, value as IModelElement, null, _validationMessage.PropertyName);
        }
    }
}