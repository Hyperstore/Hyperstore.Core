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