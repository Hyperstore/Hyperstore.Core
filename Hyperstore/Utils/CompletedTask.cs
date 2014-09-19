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
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling.Utils
{
    /// <summary>Provides access to an already completed task.</summary>
    /// <remarks>A completed task can be useful for using ContinueWith overloads where there aren't StartNew equivalents.</remarks>
    internal static class CompletedTask
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a completed Task.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static readonly Task Default = CompletedTask<object>.Default;
    }

    /// <summary>Provides access to an already completed task.</summary>
    /// <remarks>A completed task can be useful for using ContinueWith overloads where there aren't StartNew equivalents.</remarks>
    internal static class CompletedTask<TResult>
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a completed Task.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static readonly Task<TResult> Default;

        /// <summary>Initializes a Task.</summary>
        static CompletedTask()
        {
            Default = Task.FromResult<TResult>(default(TResult));
        }
    }
}