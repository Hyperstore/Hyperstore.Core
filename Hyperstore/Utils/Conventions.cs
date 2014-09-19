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
using Hyperstore.Modeling.Metadata;

#endregion

namespace Hyperstore.Modeling
{
    internal static class Conventions
    {
        internal static void CheckValidDomainName(string name)
        {
            if (name == null || name == "$")
                throw new Exception(ExceptionMessages.InvalidDomainName);

            CheckValidName(name, true);
        }

        internal static void CheckValidName(string name, bool composed = false)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new InvalidNameException(name);

            if (name[0] == '.' || Char.IsDigit(name[0]) || name.EndsWith("."))
                throw new InvalidNameException(name);

            foreach (char ch in name)
            {
                if (!Char.IsLetterOrDigit(ch) && ch != '_' && ch != '$' && !(composed && ch == '.'))
                    throw new InvalidNameException(name);
            }
        }

        internal static string ExtractMetaElementName(string domainName, string name)
        {
            if (domainName != PrimitivesSchema.DomainModelName && name.StartsWith(domainName, StringComparison.OrdinalIgnoreCase))
                return name.Substring(domainName.Length + 1);

            return name;
        }
    }
}