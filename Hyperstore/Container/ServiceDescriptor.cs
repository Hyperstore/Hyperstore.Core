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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Container
{
    internal class ServiceDescriptor
    {
        internal Type ServiceType { get; private set; }
        internal ServiceLifecycle Lifecycle { get; private set; }
        internal ServiceDescriptor Next { get; set; }

        // Only one of the following properties must be private set
        private object _serviceInstance;

        private Func<IServicesContainer, object> _factory;

        internal ServiceDescriptor(Type serviceType, ServiceLifecycle lifecycle, object instance, Func<IServicesContainer, object> factory)
        {
            Lifecycle = lifecycle;
            ServiceType = serviceType;
            _serviceInstance = instance;
            _factory = factory;
        }

        internal object Create(ServicesContainer servicesContainer)
        {
            if (_factory != null)
                return _factory(servicesContainer);

            return _serviceInstance;
        }
    }
}
