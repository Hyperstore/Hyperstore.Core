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
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Exception for signalling session errors.
    /// </summary>
    /// <seealso cref="T:System.Exception"/>
    ///-------------------------------------------------------------------------------------------------
    public class SessionException : Exception
    {
        internal SessionException(IEnumerable<DiagnosticMessage> messages)
        {
            DebugContract.Requires(messages);
            Messages = messages.ToList();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the messages.
        /// </summary>
        /// <value>
        ///  The messages.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<DiagnosticMessage> Messages { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether [has errors].
        /// </summary>
        /// <value>
        ///  <c>true</c> if [has errors]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool HasErrors
        {
            get { return Messages.Any(m => m.MessageType == MessageType.Error); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a message that describes the current exception.
        /// </summary>
        /// <value>
        ///  The error message that explains the reason for the exception, or an empty string("").
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public override string Message
        {
            get
            {
                bool first = true;
                var sb = new StringBuilder();
                foreach (var m in Messages)
                {
                    if (!first)
                        sb.AppendLine();
                    first = false;
                    sb.Append(m.ToString());
                }
                return sb.ToString();
            }
        }
    }
}