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
using System.Threading.Tasks;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Represent a service life cycle
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public enum ServiceLifecycle
    {
        /// <summary>
        ///  Only one instance per store
        /// </summary>
        Singleton,
        /// <summary>
        ///  One instance per domain or extension
        /// </summary>
        Scoped,
        /// <summary>
        ///  Always a new instance
        /// </summary>
        Transient
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for services container.
    /// </summary>
    /// <seealso cref="T:IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IServicesContainer : IDisposable
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a new scope.
        /// </summary>
        /// <returns>
        ///  An services container.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IServicesContainer NewScope();

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
        /// <param name="lifecyle">
        ///  (Optional) service life cycle
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Register<TService>(Func<IServicesContainer, TService> factory, ServiceLifecycle lifecyle = ServiceLifecycle.Scoped) where TService : class;

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