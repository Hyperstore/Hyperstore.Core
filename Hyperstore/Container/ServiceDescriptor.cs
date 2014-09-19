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
