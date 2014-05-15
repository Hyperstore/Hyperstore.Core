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
 
using System;

namespace Hyperstore.Modeling.Platform
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for reflection helper.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IReflectionHelper
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets assembly name.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  The assembly name.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        string GetAssemblyName(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets base type.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  The base type.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Type GetBaseType(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets default value.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  The default value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        object GetDefaultValue(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets generic arguments.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  The generic arguments.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Collections.Generic.IEnumerable<Type> GetGenericArguments(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the interfaces.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  The interfaces.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Collections.Generic.IEnumerable<Type> GetInterfaces(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a method.
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
        ///  The method.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Collections.Generic.IEnumerable<System.Reflection.MethodInfo> GetMethod(Type thisType, string name, params Type[] parameterTypes);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets name with simple assembly name.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  The name with simple assembly name.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        string GetNameWithSimpleAssemblyName(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets protected constructor.
        /// </summary>
        /// <param name="self">
        ///  The self.
        /// </param>
        /// <param name="types">
        ///  The types.
        /// </param>
        /// <returns>
        ///  The protected constructor.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Reflection.ConstructorInfo GetProtectedConstructor(Type self, Type[] types);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets public constructor.
        /// </summary>
        /// <param name="self">
        ///  The self.
        /// </param>
        /// <param name="types">
        ///  The types.
        /// </param>
        /// <returns>
        ///  The public constructor.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Reflection.ConstructorInfo GetPublicConstructor(Type self, Type[] types);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets uninitialized object.
        /// </summary>
        /// <param name="implementationType">
        ///  Type of the implementation.
        /// </param>
        /// <returns>
        ///  The uninitialized object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Hyperstore.Modeling.IModelElement GetUninitializedObject(Type implementationType);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'type1' is assignable from.
        /// </summary>
        /// <param name="type1">
        ///  The first type.
        /// </param>
        /// <param name="type2">
        ///  The second type.
        /// </param>
        /// <returns>
        ///  true if assignable from, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsAssignableFrom(Type type1, Type type2);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'type' is class.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  true if class, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsClass(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'type' is enum.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  true if enum, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsEnum(Type type);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'type' is generic type.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <param name="ofType">
        ///  Type of the of.
        /// </param>
        /// <returns>
        ///  true if generic type, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsGenericType(Type type, Type ofType);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'type' is value type.
        /// </summary>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  true if value type, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsValueType(Type type);
    }
}
