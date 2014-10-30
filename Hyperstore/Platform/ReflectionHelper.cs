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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

#endregion

namespace Hyperstore.Modeling.Utils
{
    internal static class ReflectionHelper
    {
        internal static string GetNameWithSimpleAssemblyName(this Type type)
        {
            DebugContract.Requires(type);

            var typeinfo = type.GetTypeInfo();
            if (!typeinfo.IsGenericType)
                return String.Format("{0}, {1}", type.FullName, GetAssemblyName(type));

            var sb = new StringBuilder();
            sb.Append(type.Namespace);
            sb.Append('.');
            sb.Append(type.Name);
            sb.Append('[');
            foreach (var t in typeinfo.GenericTypeArguments)
            {
                sb.Append('[');
                sb.Append(GetNameWithSimpleAssemblyName(t));
                sb.Append(']');
            }
            sb.Append(']');
            sb.Append(',');
            sb.Append(GetAssemblyName(type));
            return sb.ToString();
        }

        internal static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            while(!(type == typeof(Object)))
            { 
                var info = type.GetTypeInfo();
                foreach (var p in info.DeclaredProperties)
                {
                    if( p.CanRead)
                    yield return p;
                }
                type = info.BaseType;
            }
        }

        internal static IEnumerable<ConstructorInfo> GetConstructor(Type type, params Type[] types)
        {
            //var results = from m in type.GetTypeInfo().DeclaredConstructors
            //              let methodParameters = m.GetParameters().Select(_ => _.ParameterType).ToArray()
            //              where parameters.Length == 0 || (
            //                    methodParameters.Length == parameters.Length &&
            //                    !methodParameters.Except(parameters).Any() &&
            //                    !parameters.Except(methodParameters).Any())
            //              select m;

            //return results;
            foreach (var ctor in type.GetTypeInfo().DeclaredConstructors)
            {
                var parms = ctor.GetParameters();
                if (types.Length != parms.Length)
                    continue;

                bool founded = true;
                int ix = 0;
                foreach (var parm in parms)
                {
                    if (!types[ix].GetTypeInfo().IsAssignableFrom(parm.ParameterType.GetTypeInfo()))
                    {
                        founded = false;
                        break;
                    }
                    ix++;
                }
                if (founded)
                    yield return ctor;
            }
        }

        public static IEnumerable<MethodInfo> GetMethod(Type type, string methodName, params Type[] parameters)
        {
            var results = from m in type.GetTypeInfo().DeclaredMethods
                          where m.Name == methodName
                          let methodParameters = m.GetParameters().Select(_ => _.ParameterType).ToArray()
                          where parameters.Length == 0 || (
                                methodParameters.Length == parameters.Length &&
                                !methodParameters.Except(parameters).Any() &&
                                !parameters.Except(methodParameters).Any())
                          select m;

            return results;
        }

        internal static ConstructorInfo GetPublicConstructor(Type self, Type[] types)
        {
            return GetConstructor(self, types).FirstOrDefault(ctor => ctor.IsPublic);
        }

        internal static ConstructorInfo GetProtectedConstructor(Type self, Type[] types)
        {
            return GetConstructor(self, types).FirstOrDefault(ctor => ctor.IsAssembly);
        }

        /// <summary>
        ///     Special type used to match any generic parameter type in GetMethodExt().
        /// </summary>
        public class T
        {
        }

        internal static object GetDefaultValue(Type type)
        {
            var info = type.GetTypeInfo();
            if (info.IsValueType)
            {
                if (type == typeof(double))
                    return default(double);
                if (type == typeof(float))
                    return default(float);
                if (type == typeof(decimal))
                    return default(decimal);
                if (type == typeof(long))
                    return default(long);
                if (type == typeof(char))
                    return default(char);
                if (type == typeof(bool))
                    return default(bool);


                if (info.IsGenericType && info.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return null;
                if (info.IsEnum)
                    return Enum.GetValues(type).GetValue(0);
                if (type == typeof(DateTime))
                    return default(DateTime);

                return Convert.ChangeType(0, type);
            }
            return null;
        }

        internal static bool IsValueType(Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsGenericType(Type type, Type ofType)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == ofType;
        }

        internal static Type GetBaseType(Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        internal static bool IsAssignableFrom(Type type1, Type type2)
        {
            return type1.GetTypeInfo().IsAssignableFrom(type2.GetTypeInfo());
        }

        internal static IEnumerable<Type> GetInterfaces(Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        internal static IEnumerable<Type> GetGenericArguments(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GenericTypeArguments;
        }

        internal static string GetAssemblyName(Type type)
        {
            return type.GetTypeInfo().Assembly.GetName().Name;
        }

        internal static bool IsEnum(Type type)
        {
            return type != null && type.GetTypeInfo().IsEnum;
        }

        internal static bool IsClass(Type type)
        {
            return type != null && type.GetTypeInfo().IsClass;
        }
    }
}