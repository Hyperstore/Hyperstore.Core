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
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for dependency resolver.
    /// </summary>
    /// <seealso cref="T:IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IDependencyResolver : IDisposable
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers a configuration parameter.
        /// </summary>
        /// <param name="name">
        ///  The name of the setting.
        /// </param>
        /// <param name="value">
        ///  The value of the setting.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RegisterSetting(string name, object value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets setting value.
        /// </summary>
        /// <typeparam name="TSetting">
        ///  Type of the setting.
        /// </typeparam>
        /// <param name="name">
        ///  The name of the setting.
        /// </param>
        /// <returns>
        ///  The setting value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TSetting GetSettingValue<TSetting>(string name);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the specified service.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <param name="service">
        ///  Instance of the service.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Register<TService>(TService service) where TService : class;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the specified service.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <param name="factory">
        ///  The service factory.
        /// </param>
        /// <param name="singleton">
        ///  (Optional) True if the service is a singleton.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Register<TService>(Func<IDependencyResolver, TService> factory, bool singleton = false) where TService : class;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves the first active instance of a service.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TService Resolve<TService>() where TService : class;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves all the instance of a service.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process resolve all in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<TService> ResolveAll<TService>() where TService : class;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Composes this instance.
        /// </summary>
        /// <param name="assemblies">
        ///  List of assemblies to compose with, if null, takes the current executing assembly.
        /// </param>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task ComposeAsync(params System.Reflection.Assembly[] assemblies);
    }
}