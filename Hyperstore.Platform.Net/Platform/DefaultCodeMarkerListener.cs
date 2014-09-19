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