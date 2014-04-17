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