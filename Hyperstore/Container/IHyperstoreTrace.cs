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
using System.Linq;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for hyperstore trace.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IHyperstoreTrace
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Writes the trace.
        /// </summary>
        /// <param name="category">
        ///  The category.
        /// </param>
        /// <param name="format">
        ///  The format.
        /// </param>
        /// <param name="args">
        ///  The arguments.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void WriteTrace(string category, string format, params object[] args);
    }
}