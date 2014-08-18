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

#endregion

namespace Hyperstore.Modeling.Container
{
    internal sealed class DependencyFactory
    {
        private readonly Func<IDependencyResolver, object> _resolver;
        private readonly bool _singleton;
        private readonly object _sync = new object();
        private object _instance;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="instance">
        ///  The instance.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DependencyFactory(object instance)
        {
            DebugContract.Requires(instance);

            _instance = instance;
            _singleton = true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <param name="singleton">
        ///  (Optional) true to singleton.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DependencyFactory(Func<IDependencyResolver, object> resolver, bool singleton = false)
        {
            DebugContract.Requires(resolver);
            _resolver = resolver;
            _singleton = singleton;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is singleton.
        /// </summary>
        /// <value>
        ///  true if this instance is singleton, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsSingleton
        {
            get { return _singleton; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves the given dependency resolver.
        /// </summary>
        /// <param name="dependencyResolver">
        ///  The dependency resolver.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public object Resolve(IDependencyResolver dependencyResolver)
        {
            DebugContract.Requires(dependencyResolver);

            if (_singleton)
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        _instance = _resolver(dependencyResolver);
                    }
                }
                return _instance;
            }

            var obj = _resolver(dependencyResolver);
            return obj;
        }
    }
}