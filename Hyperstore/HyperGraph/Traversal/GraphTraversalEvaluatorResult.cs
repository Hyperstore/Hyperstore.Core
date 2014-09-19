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