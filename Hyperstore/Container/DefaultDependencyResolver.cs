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
using System.Threading;
using System.Threading.Tasks;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.MemoryStore;
using Hyperstore.Modeling.Messaging;
using Hyperstore.Modeling.Statistics;
using Hyperstore.Modeling.Validations;
using System.Reflection;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling.Ioc
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A default dependency resolver.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IDependencyResolverInternal"/>
    ///-------------------------------------------------------------------------------------------------
    public class DefaultDependencyResolver : IDependencyResolverInternal
    {
        private readonly Dictionary<string, InstanceInfo> _resolvers = new Dictionary<string, InstanceInfo>();
        private bool _disposed;
        private bool _initialized;
        private DefaultDependencyResolver _parent;

        private DefaultDependencyResolver(IDependencyResolver parentResolver)
        {
            _parent = parentResolver as DefaultDependencyResolver;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public DefaultDependencyResolver()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Compose asynchronous.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="assemblies">
        ///  A variable-length parameters list containing assemblies.
        /// </param>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public async Task ComposeAsync(params Assembly[] assemblies)
        {
            if (Resolve<ICompositionService>() != null)
                throw new Exception(ExceptionMessages.CompositionAlreadyDone);

            var container = Platform.PlatformServices.Current.MefContainer;
            await Task.Run(() => container.Compose(assemblies)).ConfigureAwait(false);
            Register<ICompositionService>(container);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the setting.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterSetting(string name, object value)
        {
            Contract.RequiresNotEmpty(name, "name");

            Register(new Setting(name, value));
        }


        void IDependencyResolverInternal.SetStore(IHyperstore store)
        {
            DebugContract.Requires(store);
            if (_initialized)
                throw new Exception(ExceptionMessages.AlreadyInitialized);

            _initialized = true;
            Register(store);
            Register<IStatistics>(store.Statistics);

            RegisterDefaultInstances();
            OnInitialized();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the initialized action.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void OnInitialized()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the default instances.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void RegisterDefaultInstances()
        {
            //Domain
            // IStore
            // Hyperstore.Modeling.MemoryStore.ITransactionManager
            // IPersistenceAdapterConfiguration 
            // IIndexManager ?
            // ICommandManager
            // IGraph
            // IGraphResolver
            // IQueryPipeline
            // IPersistenceAdapter<Vertex> ?
            // IModelElementResolver 

            const string platformTypeName = "Hyperstore.Modeling.Platform.PlatformServicesInstance, Hyperstore.Platform";
            var platformfactoryType = Type.GetType(platformTypeName, false);
            if (platformfactoryType != null)
            {
                Activator.CreateInstance(platformfactoryType);
            }

            // Store instance
            Register<IDependencyResolver>(this);

            // Global
            Register<ITransactionManager>(new TransactionManager(this));
            Register<IIdGenerator>(r => new GuidIdGenerator());

            // Par domain et par metamodel => Nouvelle instance à chaque fois
            Register<IHyperGraph>(r => new HyperGraph.HyperGraph(r));
            Register<IEventManager>(r => new EventManager(r));
            Register<IModelElementFactory>(r => 
                PlatformServices.Current.CreateModelElementFactory()
                );
            Register<ICommandManager>(r => new CommandManager());
            Register<IEventBus>(r => new EventBus(r));
            Register<IConstraintsManager>(r => new ConstraintsManager(r));
            Register<IEventDispatcher>(r => new EventDispatcher(r, true));
            var ctx = SynchronizationContext.Current;
            if (ctx != null)
                Register(ctx);

            Register<ISynchronizationContext>(PlatformServices.Current.CreateDispatcher());

            // découverte automatique (sans mef) de l'extension rx
            const string typeName = "Hyperstore.ReactiveExtension.SubjectFactory, Hyperstore.ReactiveExtension";
            var rxfactoryType = Type.GetType(typeName, false);
            if (rxfactoryType != null)
            {
                var factory = (ISubjectFactory)Activator.CreateInstance(rxfactoryType);
                Register(factory);
            }
        }

        #region IDependencyResolver Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers this instance.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="service">
        ///  The service.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public virtual void Register<TService>(TService service) where TService : class
        {
            Contract.Requires(service, "service");

            var factory = new DependencyFactory(service);
            RegisterFactory<TService>(factory);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers this instance.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <param name="singleton">
        ///  (Optional) true to singleton.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public virtual void Register<TService>(Func<IDependencyResolver, TService> resolver, bool singleton = false) where TService : class
        {
            Contract.Requires(resolver, "resolver");

            var factory = new DependencyFactory(c => (object)resolver(c), singleton);
            RegisterFactory<TService>(factory);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TService Resolve<TService>() where TService : class
        {
            return Resolve<TService>(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates resolve all in this collection.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process resolve all in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<TService> ResolveAll<TService>() where TService : class
        {
            return ResolveAll<TService>(this);
        }

        IDependencyResolver IDependencyResolverInternal.CreateDependencyResolver()
        {
            var resolver = new DefaultDependencyResolver(this);
            return resolver;
        }

        private static string CreateResolverKey(Type type)
        {
            DebugContract.Requires(type);
            return type.FullName;
        }

        private void RegisterFactory<TService>(DependencyFactory factory) where TService : class
        {
            DebugContract.Requires(factory);

            var key = CreateResolverKey(typeof(TService));
            InstanceInfo info;
            if (!_resolvers.TryGetValue(key, out info))
            {
                info = new InstanceInfo();
                _resolvers.Add(key, info);
            }

            info.AddFactory(this, factory);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets setting value.
        /// </summary>
        /// <typeparam name="TSetting">
        ///  Type of the setting.
        /// </typeparam>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The setting value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual TSetting GetSettingValue<TSetting>(string name)
        {
            var setting = ResolveAll<Setting>().FirstOrDefault(s => String.Compare(s.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
            return setting == null ? default(TSetting) : (TSetting)setting.Value;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual TService Resolve<TService>(IDependencyResolver owner) where TService : class
        {
            DebugContract.Requires(owner);

            var key = CreateResolverKey(typeof(TService));
            InstanceInfo info;
            if (_resolvers.TryGetValue(key, out info) || _resolvers.TryGetValue(CreateResolverKey(typeof(TService)), out info))
            {
                // On prend toujours le dernier (surcharge)
                return (TService)info.TryResolve(owner)
                        .Last();
            }

            if (_parent != null)
            {
                var parentInfo = _parent.FindRecursive<TService>(key);
                if (parentInfo != null)
                {
                    if (parentInfo.Singleton)
                    {
                        return (TService)parentInfo.TryResolve(owner)
                                .Last();
                    }

                    info = new InstanceInfo(owner, parentInfo);
                    _resolvers.Add(key, info);
                    return (TService)info.TryResolve(owner)
                            .Last();
                }
            }

            return default(TService);
        }

        private InstanceInfo FindRecursive<TService>(string key) where TService : class
        {
            var parent = this;
            do
            {
                InstanceInfo info;
                if (parent._resolvers.TryGetValue(key, out info) || parent._resolvers.TryGetValue(CreateResolverKey(typeof(TService)), out info))
                    return info;
                parent = parent._parent;
            }
            while (parent != null);

            return null;
        }

        private IEnumerable<TService> ResolveAll<TService>(IDependencyResolver owner) where TService : class
        {
            DebugContract.Requires(owner);

            var key = CreateResolverKey(typeof(TService));
            InstanceInfo info;
            if (_resolvers.TryGetValue(key, out info) || _resolvers.TryGetValue(CreateResolverKey(typeof(TService)), out info))
            {
                if (info.Singleton == false)
                    throw new Exception(ExceptionMessages.ResolveAllWorksOnlyWithSingleton);

                foreach (var instance in info.TryResolve(owner))
                {
                    yield return (TService)instance;
                }
            }

            if (_parent != null)
            {
                var parent = _parent;
                do
                {
                    if (parent._resolvers.TryGetValue(key, out info) || parent._resolvers.TryGetValue(CreateResolverKey(typeof(TService)), out info))
                    {
                        if (info.Singleton == false)
                            throw new Exception(ExceptionMessages.ResolveAllWorksOnlyWithSingleton);

                        foreach (var instance in info.TryResolve(owner))
                        {
                            yield return (TService)instance;
                        }
                    }

                    parent = parent._parent;
                }
                while (parent != null);
            }
        }

        #endregion

        #region IDisposable Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the
        ///  Hyperstore.Modeling.Ioc.DefaultDependencyResolver and optionally releases the managed
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the
        ///  Hyperstore.Modeling.Ioc.DefaultDependencyResolver and optionally releases the managed
        ///  resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (!_disposed)
            {
                lock (_resolvers)
                {
                    if (!_disposed)
                    {
                        _disposed = true;
                        foreach (var i in _resolvers.Values)
                        {
                            i.Dispose();
                        }
                    }

                    _resolvers.Clear();
                    _parent = null;
                    _resolvers.Clear();
                }
            }
        }

        #endregion

        /// <summary>
        ///     Enregistre les factories et les instances au niveau d'un domaine.
        ///     Toutes les factories d'un même type doivent avoir le même comportement (tous singleton ou aucun)
        ///     La résolution d'un type entraine la création de toutes les factories de ce type.
        ///     Une fois résolu, il n'est plus possible d'ajouter des factories à un type.
        ///     Un singleton reste au niveau qui l'a défini.
        ///     Sinon on crée une instance au niveau du domaine qui le demande, cette instance reste unique pour tous les domaines
        ///     et/ou le modèle
        /// </summary>
        private class InstanceInfo : IDisposable
        {
            private IList<object> _instances;
            private bool _resolved;
            private IList<DependencyFactory> _resolvers;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Default constructor.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public InstanceInfo()
            {
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="resolver">
            ///  The resolver.
            /// </param>
            /// <param name="parentInfo">
            ///  Information describing the parent.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public InstanceInfo(IDependencyResolver resolver, InstanceInfo parentInfo)
            {
                Singleton = parentInfo.Singleton;
                _resolvers = new List<DependencyFactory>();
                _instances = new List<object>();
                ResolveAll(resolver, parentInfo._resolvers);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets a value indicating whether the singleton.
            /// </summary>
            /// <value>
            ///  true if singleton, false if not.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public bool Singleton { get; private set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
            ///  resources.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Dispose()
            {
                foreach (var d in _instances)
                {
                    var disposable = d as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                _instances.Clear();
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Adds a factory to 'factory'.
            /// </summary>
            /// <exception cref="Exception">
            ///  Thrown when an exception error condition occurs.
            /// </exception>
            /// <param name="owner">
            ///  The owner.
            /// </param>
            /// <param name="factory">
            ///  The factory.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void AddFactory(IDependencyResolver owner, DependencyFactory factory)
            {
                if (_resolvers == null)
                {
                    Singleton = factory.IsSingleton;
                    _resolvers = new List<DependencyFactory>();
                    _instances = new List<object>();
                }

                if (_resolved)
                    throw new Exception(ExceptionMessages.UnableToRegisterServiceAfterCallToResolveOrResolveAll);

                if (factory.IsSingleton != Singleton)
                    throw new Exception(ExceptionMessages.TypeFactoriesMustHaveSameBehavior);

                if (factory.IsSingleton)
                    _instances.Add(factory.Resolve(owner));
                else
                    _resolvers.Add(factory);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Enumerates try resolve in this collection.
            /// </summary>
            /// <param name="resolver">
            ///  The resolver.
            /// </param>
            /// <returns>
            ///  An enumerator that allows foreach to be used to process try resolve in this collection.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public IEnumerable<object> TryResolve(IDependencyResolver resolver)
            {
                if (!_resolved)
                {
                    var resolvers = _resolvers;
                    ResolveAll(resolver, resolvers);
                }

                return _instances;
            }

            private void ResolveAll(IDependencyResolver resolver, IEnumerable<DependencyFactory> resolvers)
            {
                foreach (var factory in resolvers)
                {
                    _instances.Add(factory.Resolve(resolver));
                }
                _resolved = true;
            }
        }
    }
}