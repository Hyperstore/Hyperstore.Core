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
        ///  Creates a message.
        /// </summary>
        /// <param name="level">
        ///  Type message
        /// </param>
        /// <param name="format">
        ///  Describes the format to use.
        /// </param>
        /// <param name="args">
        ///  A variable-length parameters list containing arguments.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void CreateMessage( MessageType level, string format, params object[] args)
        {
            var message = args.Length == 0 ? format : String.Format(format, args);
            _sessionContext.Log(new DiagnosticMessage(level, MessageHelper.CreateMessage(message, Element), _category, true, null, null));
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
