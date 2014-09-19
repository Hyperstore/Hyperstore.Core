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

namespace Hyperstore.Modeling
{
    internal static class Contract
    {
        [DebuggerStepThrough]
        internal static void Requires(bool value, string parameter)
        {
            if (!value)
                throw new ArgumentNullException(parameter);
        }

        [DebuggerStepThrough]
        internal static void Requires<T>(T value, string parameter) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(parameter);
        }

        [DebuggerStepThrough]
        internal static void RequiresNotEmpty(string value, string parameter)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException(parameter);
        }
    }
}