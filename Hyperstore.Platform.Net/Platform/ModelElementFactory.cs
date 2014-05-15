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
using System.Reflection;
using Hyperstore.Modeling.Utils;
using Hyperstore.Modeling;
using System.Runtime.Serialization;

#endregion

namespace Hyperstore.Platform.Net
{
    internal class ModelElementFactory : Hyperstore.Modeling.Domain.ModelElementFactory
    {
        protected override IModelElement InstanciateModelElementCore(Type implementationType)
        {
            return (IModelElement)FormatterServices.GetUninitializedObject(implementationType);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve type.
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">
        ///  Name of the assembly qualified type.
        /// </param>
        /// <returns>
        ///  A Type.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override Type ResolveType(string assemblyQualifiedTypeName)
        {
            DebugContract.RequiresNotEmpty(assemblyQualifiedTypeName);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            try
            {
                return Type.GetType(assemblyQualifiedTypeName);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var pos = args.Name.IndexOf(',');
            var name = pos > 0
                    ? args.Name.Substring(0, pos)
                            .Trim()
                    : args.Name;
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName()
                            .Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            return asm;
        }
    }
}