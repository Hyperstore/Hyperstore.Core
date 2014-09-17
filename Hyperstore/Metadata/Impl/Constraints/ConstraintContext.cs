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
    public sealed class ConstraintContext 
    {
        private ISessionResult _messages;
        private ISessionContext _sessionContext;
        private readonly string _category;

        internal ConstraintContext(ISessionContext sessionContext, string category)
        {
            DebugContract.Requires(sessionContext);
            DebugContract.RequiresNotEmpty(category);

            this._sessionContext = sessionContext;
            this._messages = _sessionContext.Result;
            this._category = category;
        }

        public void CreateErrorMessage(string message, string propertyName = null, Exception ex=null)
        {
            _sessionContext.Log(new DiagnosticMessage(MessageType.Error, MessageHelper.CreateMessage(message, Element), _category, true, Element, ex, propertyName ?? PropertyName));
        }

        public void CreateWarningMessage(string message,  string propertyName = null)
        {
            _sessionContext.Log(new DiagnosticMessage(MessageType.Warning, MessageHelper.CreateMessage(message, Element), _category, true, Element, null, propertyName ?? PropertyName));
        }

        public ISessionResult Messages
        {
            get { return _messages; }
        }

        public string PropertyName
        {
            get;
            internal set;
        }

        public IModelElement Element
        {
            get;
            internal set;
        }
    }
}
