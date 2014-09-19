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
using System.Diagnostics;

#endregion

namespace Hyperstore.Modeling.Utils
{
    [DebuggerNonUserCode]
    internal class MarkerEntry
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="text">
        ///  The text.
        /// </param>
        /// <param name="timestamp">
        ///  The timestamp.
        /// </param>
        /// <param name="threadId">
        ///  The identifier of the thread.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public MarkerEntry(string text, DateTime timestamp, int threadId)
        {
            Text = text;
            Timestamp = timestamp;
            ThreadId = threadId;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the text.
        /// </summary>
        /// <value>
        ///  The text.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Text { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the thread.
        /// </summary>
        /// <value>
        ///  The identifier of the thread.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int ThreadId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the Date/Time of the timestamp.
        /// </summary>
        /// <value>
        ///  The timestamp.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public DateTime Timestamp { get; private set; }
    }
}