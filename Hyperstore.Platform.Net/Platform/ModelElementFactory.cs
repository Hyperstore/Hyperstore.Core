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
using System.Reflection;
using Hyperstore.Modeling.Utils;
using Hyperstore.Modeling;
using System.Runtime.Serialization;

#endregion

namespace Hyperstore.Modeling.Platform.Net
{
    internal class ModelElementFactory : global::Hyperstore.Modeling.Domain.ModelElementFactory
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