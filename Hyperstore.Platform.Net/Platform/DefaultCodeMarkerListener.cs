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

using Hyperstore.Modeling.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

#endregion

namespace Hyperstore.Modeling.Platform.Net
{
    internal class DefaultCodeMarkerListener : ICodeMarkerListener
    {
        private FileStream _logFileStream;

        private StreamWriter _logWriter;

        private readonly object _syncObject = new object();

        private const string CodeMarkerFileName = "Hyperstore";

        private const string Extension = ".log";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Logs.
        /// </summary>
        /// <param name="text">
        ///  The text.
        /// </param>
        /// <param name="timeStamp">
        ///  The time stamp Date/Time.
        /// </param>
        /// <param name="threadId">
        ///  Identifier for the thread.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Log(string text, DateTime timeStamp, int threadId)
        {
            var str = string.Format(CultureInfo.CurrentCulture, "Marker: {0}#{1}!{2}/{3}/{4} {5}:{6}:{7}.{8:D3}", text, threadId, timeStamp.Month, timeStamp.Day, timeStamp.Year, timeStamp.Hour, timeStamp.Minute, timeStamp.Second,
                    timeStamp.Millisecond);

            lock (_syncObject)
            {
                if (_logFileStream == null)
                {
                    _logFileStream = new FileStream(string.Format(CultureInfo.CurrentCulture, "{0}-{1}-{2}{3}", CodeMarkerFileName, Process.GetCurrentProcess()
                            .Id, AppDomain.CurrentDomain.Id, Extension), FileMode.CreateNew);

                    _logWriter = new StreamWriter(_logFileStream);
                }

                _logWriter.WriteLine(str);
            }

            if (_logWriter != null)
                _logWriter.Flush();

            if (_logFileStream != null)
                _logFileStream.Flush();
        }
    }
}