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
#if NETFX_CORE
    internal static class ReflectionHelper
    {
        internal static string GetNameWithSimpleAssemblyName(this Type type)
        {
            DebugContract.Requires(type);
            var typeinfo = type.GetTypeInfo();
            if( !typeinfo.IsGenericType)
                return String.Format("{0}, {1}", type.FullName, GetAssemblyName(type));

            var sb = new StringBuilder();
            sb.Append(type.Namespace);
            sb.Append('.');
            sb.Append(type.Name);
            sb.Append('[');
            foreach(var t in typeinfo.GenericTypeArguments)
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

        private static IEnumerable<ConstructorInfo> GetConstructor(Type type, params Type[] types)
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

        internal static IModelElement GetUninitializedObject(Type implementationType)
        {
            // TODO a revoir
            var ctor = GetConstructor(implementationType).FirstOrDefault(c => c.GetParameters().Length == 0);
            if (ctor == null)
                throw new Exception(ExceptionMessages.ElementMustHaveAProtectedParameterlessConstructor);
            return (IModelElement)ctor.Invoke(null);
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
#else
    internal static class ReflectionHelper
    {
        internal static string GetNameWithSimpleAssemblyName(this Type type)
        {
            DebugContract.Requires(type);
            if( !type.IsGenericType)
                return String.Format("{0}, {1}", type.FullName, GetAssemblyName(type));

            var sb = new StringBuilder();
            sb.Append(type.Namespace);
            sb.Append('.');
            sb.Append(type.Name);
            sb.Append('[');
            foreach(var t in type.GetGenericArguments())
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

        internal static object GetDefaultValue(Type type)
        {
            var info = type;
            if (info.IsValueType)
            {
                if (type == typeof (double))
                    return default(double);
                if (type == typeof (float))
                    return default(float);
                if (type == typeof (decimal))
                    return default(decimal);
                if (type == typeof (long))
                    return default(long);
                if (type == typeof (char))
                    return default(char);
                if (type == typeof (bool))
                    return default(bool);
                if (type == typeof(Guid))
                    return default(Guid);
                if (info == typeof(DateTime))
                    return default(DateTime);
                if (info == typeof(TimeSpan))
                    return default(TimeSpan);

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
                    return null;

                if (info.IsEnum)
                {
                    return Enum.GetValues(type)
                            .GetValue(0);
                }

                return Convert.ChangeType(0, type);
            }
            return null;
        }

        internal static IModelElement GetUninitializedObject(Type implementationType)
        {
            return (IModelElement) FormatterServices.GetUninitializedObject(implementationType);
        }

        internal static string GetAssemblyName(Type type)
        {
            return type.Assembly.GetName().Name;
        }

        internal static IEnumerable<Type> GetGenericArguments(Type type)
        {
            return type.GetGenericArguments();
        }

        internal static IEnumerable<Type> GetInterfaces(Type type)
        {
            return type.GetInterfaces();
        }

        internal static bool IsAssignableFrom(Type type1, Type type2)
        {
            return type1.IsAssignableFrom(type2);
        }

        internal static Type GetBaseType(Type type)
        {
            return type.BaseType;
        }

        internal static bool IsValueType(Type type)
        {
            return type.IsValueType;
        }

        internal static bool IsGenericType(Type type, Type ofType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == ofType;
        }

        internal static ConstructorInfo GetPublicConstructor(Type self, Type[] types)
        {
            foreach (var ctor in self.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var parms = ctor.GetParameters();
                if (types.Length != parms.Length)
                    continue;

                var founded = true;
                var ix = 0;
                foreach (var parm in parms)
                {
                    if (!types[ix].IsAssignableFrom(parm.ParameterType))
                    {
                        founded = false;
                        break;
                    }
                    ix++;
                }
                if (founded)
                    return ctor;
            }
            return null;
        }

        internal static ConstructorInfo GetProtectedConstructor(Type self, Type[] types)
        {
            return self.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets custom attributes.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="provider">
        ///  The provider.
        /// </param>
        /// <param name="inherited">
        ///  (Optional) true if inherited.
        /// </param>
        /// <returns>
        ///  The custom attributes.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static List<T> GetCustomAttributes<T>(ICustomAttributeProvider provider, bool inherited = false) where T : Attribute
        {
            DebugContract.Requires(provider);
            return provider.GetCustomAttributes(typeof (T), inherited)
                    .Cast<T>()
                    .ToList();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Search for a method by name and parameter types.  Unlike GetMethod(), does 'loose' matching
        ///  on generic parameter types, and searches base interfaces.
        /// </summary>
        /// <param name="thisType">
        ///  Type of this.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="parameterTypes">
        ///  List of types of the parameters.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the methods in this collection.
        /// </returns>
        /// ### <exception cref="System.Reflection.AmbiguousMatchException">
        ///  .
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        public static IEnumerable<MethodInfo> GetMethod(Type thisType, string name, params Type[] parameterTypes)
        {
            DebugContract.Requires(thisType);
            DebugContract.RequiresNotEmpty(name);

            return GetMethod(thisType, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, parameterTypes);
        }

        /// <summary>
        ///     Search for a method by name, parameter types, and binding flags.  Unlike GetMethod(), does 'loose' matching on
        ///     generic
        ///     parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="System.Reflection.AmbiguousMatchException" />
        private static IEnumerable<MethodInfo> GetMethod(Type thisType, string name, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            DebugContract.Requires(thisType);
            DebugContract.RequiresNotEmpty(name);

            // Check all methods with the specified name, including in base classes
            foreach (var mi in GetMethodInternal(thisType, name, bindingFlags, parameterTypes))
            {
                yield return mi;
            }

            // If we're searching an interface, we have to manually search base interfaces
            if (thisType.IsInterface)
            {
                foreach (var interfaceType in thisType.GetInterfaces())
                {
                    foreach (var mi in GetMethodInternal(interfaceType, name, bindingFlags, parameterTypes))
                    {
                        yield return mi;
                    }
                }
            }
        }

        private static IEnumerable<MethodInfo> GetMethodInternal(Type type, string name, BindingFlags bindingFlags, params Type[] parameterTypes)
        {
            DebugContract.Requires(type);
            DebugContract.RequiresNotEmpty(name);

            // Check all methods with the specified name, including in base classes
            foreach (MethodInfo methodInfo in type.GetMember(name, MemberTypes.Method, bindingFlags))
            {
                if (parameterTypes.Length == 0)
                    yield return methodInfo;
                else
                {
                    // Check that the parameter counts and types match, with 'loose' matching on generic parameters
                    var parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos.Length == parameterTypes.Length)
                    {
                        var i = 0;
                        for (; i < parameterInfos.Length; ++i)
                        {
                            if (!parameterInfos[i].ParameterType.IsSimilarType(parameterTypes[i]))
                                break;
                        }
                        if (i == parameterInfos.Length)
                            yield return methodInfo;
                    }
                }
            }
        }

        /// <summary>
        ///     Determines if the two types are either identical, or are both generic parameters or generic types
        ///     with generic parameters in the same locations (generic parameters match any other generic paramter,
        ///     but NOT concrete types).
        /// </summary>
        private static bool IsSimilarType(this Type thisType, Type type)
        {
            DebugContract.Requires(thisType);
            DebugContract.Requires(type);

            // Ignore any 'ref' types
            if (thisType.IsByRef)
                thisType = thisType.GetElementType();
            if (type.IsByRef)
                type = type.GetElementType();

            // Handle array types
            if (thisType.IsArray && type.IsArray)
            {
                return thisType.GetElementType()
                        .IsSimilarType(type.GetElementType());
            }

            // If the types are identical, or they're both generic parameters or the special 'T' type, treat as a match
            if (thisType == type || ((thisType.IsGenericParameter || thisType == typeof (T)) && (type.IsGenericParameter || type == typeof (T))))
                return true;

            // Handle any generic arguments
            if (thisType.IsGenericType && type.IsGenericType)
            {
                var thisArguments = thisType.GetGenericArguments();
                var arguments = type.GetGenericArguments();
                if (thisArguments.Length == arguments.Length)
                {
                    for (var i = 0; i < thisArguments.Length; ++i)
                    {
                        if (!thisArguments[i].IsSimilarType(arguments[i]))
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }

        internal static bool IsEnum(Type type)
        {
            return type != null && type.IsEnum;
        }

        internal static bool IsClass(Type type)
        {
            return type != null && type.IsClass;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Special type used to match any generic parameter type in GetMethodExt().
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public class T
        {
        }
    }
#endif
}