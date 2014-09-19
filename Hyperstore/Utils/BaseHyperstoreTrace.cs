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
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A base hyperstore trace.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IHyperstoreTrace"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class BaseHyperstoreTrace : IHyperstoreTrace
    {
        private readonly string _outputCategory;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="outputCategory">
        ///  Category the output belongs to.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected BaseHyperstoreTrace(string outputCategory)
        {
            _outputCategory = outputCategory;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'category' is enabled.
        /// </summary>
        /// <param name="category">
        ///  The category.
        /// </param>
        /// <returns>
        ///  true if enabled, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsEnabled(string category)
        {
            if (_outputCategory == null)
                return false;

            return (_outputCategory == "*" || _outputCategory == category);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Writes the trace.
        /// </summary>
        /// <param name="category">
        ///  The category.
        /// </param>
        /// <param name="format">
        ///  Describes the format to use.
        /// </param>
        /// <param name="args">
        ///  A variable-length parameters list containing arguments.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void WriteTrace(string category, string format, params object[] args)
        {            
            if (IsEnabled( category))
            {
                string message = args.Length == 0 ? format : String.Format("{3} [{0}] {1:H:mm:ss.fff} - {2} ", ThreadHelper.CurrentThreadId, DateTime.Now, String.Format(format, args), category);
                WriteMessage(message);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Writes a message.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected abstract void WriteMessage(string message);
    }
}