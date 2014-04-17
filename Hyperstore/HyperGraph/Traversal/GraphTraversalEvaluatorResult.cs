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

#endregion

namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Traversal evaluator result.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum GraphTraversalEvaluatorResult
    {
        /// <summary>
        ///     Take the current path
        /// </summary>
        Include = 0x10,
        /// <summary>
        ///     Continue with this path (drill down)
        /// </summary>
        Continue = 0x01,
        /// <summary>
        ///     Stop the navigation and terminate the iteration
        /// </summary>
        Exit = 0x100,
        /// <summary>
        ///     Take the current path and continue with this path
        /// </summary>
        IncludeAndContinue = Include | Continue,
        /// <summary>
        ///     Take the current path, stop to navigate with this path and continue with the next path
        /// </summary>
        IncludeAndNextPath = Include,
        /// <summary>
        ///     Ignore the current path and continue with this path
        /// </summary>
        ExcludeAndContinue = Continue,
        /// <summary>
        ///     Ignore the current path, stop to navigate with this path and continue with the next path
        /// </summary>
        ExcludeAndNextPath = 0,
        /// <summary>
        ///     Take the current path and stop the query
        /// </summary>
        IncludeAndExit = Include | Exit,
        /// <summary>
        ///     Ignore the current path and stop the query
        /// </summary>
        ExcludeAndExit = Exit
    }
}