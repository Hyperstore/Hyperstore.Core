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

using Hyperstore.Modeling.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Container
{
    class ServicesContainer : IServicesContainer
    {
        private readonly ServicesContainer _parent;
        private readonly ThreadSafeLazyRef<ImmutableDictionary<Type, ServiceDescriptor>> _services = new ThreadSafeLazyRef<ImmutableDictionary<Type, ServiceDescriptor>>(() => ImmutableDictionary<Type, ServiceDescriptor>.Empty);
        private readonly ThreadSafeLazyRef<ImmutableDictionary<ServiceDescriptor, object>> _resolvedServices = new ThreadSafeLazyRef<ImmutableDictionary<ServiceDescriptor, object>>(() => ImmutableDictionary<ServiceDescriptor, object>.Empty);


        internal ServicesContainer(ServicesContainer parent = null)
        {
            _parent = parent;
            Register<IServicesContainer>(this);
        }

        public IServicesContainer NewScope()
        {
            var services = new ServicesContainer(this);
            return services;
        }

        private void Register(ServiceDescriptor descriptor)
        {
            _services.ExchangeValue(services =>
                {
                    ServiceDescriptor desc;
                    if (!services.TryGetValue(descriptor.ServiceType, out desc))
                    {
                        return services.Add(descriptor.ServiceType, descriptor);
                    }

                    descriptor.Next = desc;
                    return services.SetItem(descriptor.ServiceType, descriptor);
                });
        }

        public void RegisterSetting(string name, object value)
        {
            Contract.RequiresNotEmpty(name, "name");

            Register(new Setting(name, value));
        }

        public TSetting GetSettingValue<TSetting>(string name)
        {
            var setting = ResolveAll<Setting>().FirstOrDefault(s => String.Compare(s.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
            return setting == null ? default(TSetting) : (TSetting)setting.Value;
        }

        public void Register<TService>(TService service) where TService : class
        {
            Register(new ServiceDescriptor(typeof(TService), ServiceLifecycle.Singleton, service, null));
        }

        public void Register<TService>(Func<IServicesContainer, TService> factory, ServiceLifecycle lifecyle = ServiceLifecycle.Scoped) where TService : class
        {
            Register(new ServiceDescriptor(typeof(TService), lifecyle, null, factory));
        }

        public TService Resolve<TService>() where TService : class
        {
            ServiceDescriptor desc;
            if (_services.HasValue && _services.Value.TryGetValue(typeof(TService), out desc))
            {
                return ResolveService<TService>(desc);
            }

            if (_parent != null)
            {
                desc = _parent.TryGetService<TService>();
                if( desc != null && desc.Lifecycle == ServiceLifecycle.Scoped)
                {
                    Register(desc);
                    return ResolveService<TService>(desc);
                }

                return _parent.Resolve<TService>();
            }

            return default(TService);
        }

        private ServiceDescriptor TryGetService<TService>() where TService : class
        {
            var parent = this;
            while (parent != null)
            {
                ServiceDescriptor desc;
                if (_services.HasValue && _services.Value.TryGetValue(typeof(TService), out desc))
                    return desc;
                parent = parent._parent;
            }
            return null;
        }

        private TService ResolveService<TService>(ServiceDescriptor desc) where TService : class
        {
            if (desc.Lifecycle == ServiceLifecycle.Singleton && _parent != null)
            {
                return _parent.ResolveService<TService>(desc);
            }
            else if (desc.Lifecycle == ServiceLifecycle.Transient)
            {
                return (TService)desc.Create(this); // TODO disposable
            }

            _resolvedServices.ExchangeValue(services =>
                {
                    if (!services.ContainsKey(desc))
                    {
                        return services.Add(desc, desc.Create(this));
                    }
                    return services;
                });

            return (TService)_resolvedServices.Value[desc];
        }

        public IEnumerable<TService> ResolveAll<TService>() where TService : class
        {
            ServiceDescriptor desc;
            if (_services.Value.TryGetValue(typeof(TService), out desc))
            {
                var next = desc;
                while (next != null)
                {
                    yield return ResolveService<TService>(next);
                    next = next.Next;
                }
            }

            if (_parent != null)
            {
                foreach (var service in _parent.ResolveAll<TService>())
                {
                    yield return service;
                }
            }
        }

        public async Task ComposeAsync(params System.Reflection.Assembly[] assemblies)
        {
            if (Resolve<ICompositionService>() != null)
                throw new Exception(ExceptionMessages.CompositionAlreadyDone);

            var container = new Hyperstore.Modeling.Container.Composition.CompositionContainer();
            await Task.Run(() => container.Compose(assemblies)).ConfigureAwait(false);
            Register<ICompositionService>(container);
        }

        public void Dispose()
        {
            if (!_resolvedServices.HasValue)
                return;

            var list = _resolvedServices.Value;
            foreach (var disposable in list.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}
