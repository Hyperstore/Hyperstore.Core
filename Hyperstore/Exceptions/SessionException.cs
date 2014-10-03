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