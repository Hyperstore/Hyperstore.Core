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