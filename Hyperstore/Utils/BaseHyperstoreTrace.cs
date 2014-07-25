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