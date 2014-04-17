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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

#endregion

namespace Hyperstore.Modeling.Validations
{
    internal class ExecutionResult : IExecutionResult, IExecutionResultInternal
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The empty.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static readonly ExecutionResult Empty = new ExecutionResult();

        #region Fields

        private readonly List<DiagnosticMessage> _messages = new List<DiagnosticMessage>();
        private bool _silentMode;

        #endregion

        #region Properties

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether there is no error messages and silent Mode is false.
        /// </summary>
        /// <value>
        ///  <c>true</c> if [has errors]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool HasErrors
        {
            get { return !_silentMode && _messages.Any(m => m.MessageType == MessageType.Error); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the messages.
        /// </summary>
        /// <value>
        ///  The messages.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<DiagnosticMessage> Messages
        {
            get { return new ReadOnlyCollection<DiagnosticMessage>(_messages); }
        }

        #endregion

        #region Methods

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set the current session in silent mode. No exception will be raised at the end of the session.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void SetSilentMode()
        {
            _silentMode = true;
        }

        IExecutionResult IExecutionResultInternal.Merge(IExecutionResult other)
        {
            DebugContract.Requires(other);
            _messages.AddRange(other.Messages);
            var messageList = other as ExecutionResult;
            if (messageList != null)
                _silentMode |= messageList._silentMode;
            return messageList;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a message.
        /// </summary>
        /// <param name="msg">
        ///  The message.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void AddMessage(DiagnosticMessage msg)
        {
            DebugContract.Requires(msg);
            _messages.Add(msg);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var m in _messages)
            {
                sb.AppendLine(m.ToString());
            }
            return sb.ToString();
        }

        #endregion

        internal void AddMessages(IExecutionResult list)
        {
            foreach (var msg in list.Messages)
            {
                AddMessage(msg);
            }
        }
    }
}