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
    internal static class Types
    {
        internal static Tuple<string, string> SplitFullName(string name)
        {
            DebugContract.RequiresNotEmpty(name);

            var pos = name.LastIndexOf('.');
            if (pos < 0)
                return Tuple.Create(String.Empty, name);

            return Tuple.Create(name.Substring(0, pos), name.Substring(pos + 1));
        }
    }
}