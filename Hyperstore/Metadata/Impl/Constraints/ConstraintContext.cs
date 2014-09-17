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
    ///  A constraint context.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public sealed class ConstraintContext 
    {
        private ISessionResult _messages;
        private ISessionContext _sessionContext;
        private readonly string _category;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the constraint kind.
        /// </summary>
        /// <value>
        ///  The constraint kind.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintKind ConstraintKind { get; private set; }

        internal ConstraintContext(ISessionContext sessionContext, string category, ConstraintKind kind)
        {
            DebugContract.Requires(sessionContext);
            DebugContract.RequiresNotEmpty(category);

            this._sessionContext = sessionContext;
            this._messages = _sessionContext.Result;
            this._category = category;
            this.ConstraintKind = kind;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates error message.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) The name of the property.
        /// </param>
        /// <param name="ex">
        ///  (Optional) the ex.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void CreateErrorMessage(string message, string propertyName = null, Exception ex=null)
        {
            _sessionContext.Log(new DiagnosticMessage(MessageType.Error, MessageHelper.CreateMessage(message, Element), _category, true, Element, ex, propertyName ?? PropertyName));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates warning message.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) The name of the property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void CreateWarningMessage(string message,  string propertyName = null)
        {
            _sessionContext.Log(new DiagnosticMessage(MessageType.Warning, MessageHelper.CreateMessage(message, Element), _category, true, Element, null, propertyName ?? PropertyName));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the messages.
        /// </summary>
        /// <value>
        ///  The messages.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISessionResult Messages
        {
            get { return _messages; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name of the property.
        /// </summary>
        /// <value>
        ///  The name of the property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string PropertyName
        {
            get;
            internal set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <value>
        ///  The element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement Element
        {
            get;
            internal set;
        }
    }
}
