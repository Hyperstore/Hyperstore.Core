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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Messaging;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Statistics;
using Hyperstore.Modeling.Validations;

#endregion

namespace Hyperstore.Modeling.Domain
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A data Model for the domain.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IUpdatableDomainModel"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("Domain {Name} {ExtensionName}")]
    public class DomainModel : IUpdatableDomainModel, IHyperGraphProvider
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly object _resolversLock = new object();
        private bool _disposed;

        IHyperGraph IHyperGraphProvider.InnerGraph { get { return ((DomainModel)this).InnerGraph; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The inner graph.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected IHyperGraph InnerGraph;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the 1 cache.
        /// </summary>
        /// <value>
        ///  The l 1 cache.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected Level1Cache L1Cache { get; private set; }

        private ICommandManager _commandManager;
        private IEventManager _eventManager;
        private bool _initialized;
        private IModelElementFactory _modelElementFactory;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="dependencyResolver">
        ///  The dependency resolver.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainModel(IDependencyResolver dependencyResolver, string name)
        {
            Contract.Requires(dependencyResolver, "dependencyResolver");
            Contract.RequiresNotEmpty(name, "name");

            InstanceId = Guid.NewGuid().ToString("N");

            Name = name.ToLower();
            _dependencyResolver = dependencyResolver;
            Store = DependencyResolver.Resolve<IHyperstore>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'domain' is same.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <returns>
        ///  true if same, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsSame(IDomainModel domain)
        {
            return domain != null && String.Compare(Name, domain.Name, StringComparison.OrdinalIgnoreCase) == 0 && String.Compare(ExtensionName, domain.ExtensionName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in DomainLoaded events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler DomainLoaded;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in DomainUnloaded events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler DomainUnloaded;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the instance.
        /// </summary>
        /// <value>
        ///  The identifier of the instance.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string InstanceId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the event dispatcher.
        /// </summary>
        /// <value>
        ///  The event dispatcher.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Hyperstore.Modeling.Events.IEventDispatcher EventDispatcher { get; set; }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the statistics.
        /// </summary>
        /// <value>
        ///  The statistics.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public DomainStatistics Statistics { get; private set; }

        void IDomainModel.Configure()
        {
            ConfigureCore();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines if we can configure core.
        /// </summary>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual bool ConfigureCore()
        {
            if (_initialized)
                return true;

            _initialized = true;
            Statistics = new DomainStatistics();

            OnInitializing();

            _eventManager = Resolve(ResolveEventManager);
            IdGenerator = Resolve(ResolveIdGenerator);
            _modelElementFactory = Resolve(ResolveModelElementFactory);
            _commandManager = Resolve(ResolveCommandManager);
            InnerGraph = Resolve(ResolveHyperGraph);
            L1Cache = new Level1Cache(this);

            OnInitialized();

            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a new identifier.
        /// </summary>
        /// <param name="schemaElement">
        ///  (Optional) the schema element.
        /// </param>
        /// <param name="key">
        ///  (Optional) the key.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Identity CreateId(string key = null, ISchemaElement schemaElement = null)
        {
            if (key == null)
                return IdGenerator.NextValue(schemaElement);

            return new Identity(this.Name, key);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a new identifier.
        /// </summary>
        /// <param name="key">
        ///  the key.
        /// </param>
        /// <param name="schemaElement">
        ///  (Optional) the schema element.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Identity CreateId(long key, ISchemaElement schemaElement = null)
        {
            return CreateId(key.ToString(), schemaElement);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="throwExceptionIfNotExists">
        ///  (Optional) true to throw exception if not exists.
        /// </param>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TService Resolve<TService>(bool throwExceptionIfNotExists = true) where TService : class
        {
            return Resolve(() => DependencyResolver.Resolve<TService>(), throwExceptionIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.Domain.DomainModel and
        ///  optionally releases the managed resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///  true if this instance is disposed, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsDisposed
        {
            get { return _disposed; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the events.
        /// </summary>
        /// <value>
        ///  The events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEventManager Events
        {
            [DebuggerStepThrough]
            get
            {
                CheckInitialized();
                return _eventManager;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the commands.
        /// </summary>
        /// <value>
        ///  The commands.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual ICommandManager Commands
        {
            [DebuggerStepThrough]
            get
            {
                CheckInitialized();
                return _commandManager;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the model element resolver.
        /// </summary>
        /// <value>
        ///  The model element resolver.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual IModelElementFactory ModelElementFactory
        {
            get { return _modelElementFactory; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the dependency resolver.
        /// </summary>
        /// <value>
        ///  The dependency resolver.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDependencyResolver DependencyResolver
        {
            get { return _dependencyResolver; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads an extension.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="extensionName">
        ///  The name of the extension.
        /// </param>
        /// <param name="configuration">
        ///  (Optional) the configuration.
        /// </param>
        /// <returns>
        ///  The extension.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual async Task<IDomainModelExtension> LoadExtensionAsync(string extensionName, IDomainConfiguration configuration = null)
        {
            Contract.RequiresNotEmpty(extensionName, "extensionName");
            CheckInitialized();

            if (this is ISchema)
                throw new Exception("Uses LoadSchemaExtension for a schema");

            Conventions.CheckValidDomainName(extensionName);
            if ((Store.Options & StoreOptions.EnableExtensions) != StoreOptions.EnableExtensions)
                throw new Exception("Extensions are not enabled. Use StoreOptions.EnableExtensions when instancing the store.");

            var domain = await Store.CreateDomainModelAsync(extensionName, configuration, (resolver, name) => new DomainExtension.DomainModelExtension(resolver, ((IDomainModel)this).Name, extensionName, this));
            return (IDomainModelExtension)domain;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve or register singleton.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="service">
        ///  The service.
        /// </param>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TService ResolveOrRegisterSingleton<TService>(TService service) where TService : class
        {
            var result = Resolve<TService>(false);
            if (result != null)
                return result;

            lock (_resolversLock)
            {
                result = Resolve<TService>(false);
                if (result == null)
                    DependencyResolver.Register(service);

                return result ?? service;
            }
        }

        IModelElement IUpdatableDomainModel.CreateEntity(Identity id, ISchemaEntity metaClass, IModelEntity instance)
        {
            return CreateEntityCore(id, metaClass, instance);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates entity core.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaClass">
        ///  the meta class.
        /// </param>
        /// <param name="instance">
        ///  The instance.
        /// </param>
        /// <returns>
        ///  The new entity core.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IModelElement CreateEntityCore(Identity id, ISchemaEntity metaClass, IModelEntity instance)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaClass, "metaClass");
            IModelElement result = instance;

            CheckInitialized();

            using (var session = EnsuresRunInSession())
            {
                result = L1Cache.CreateEntity(id, metaClass, instance);
                if (session != null)
                    session.AcceptChanges();
                return result;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Check initialized.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        protected void CheckInitialized()
        {
            if (!_initialized)
                throw new Exception("Domain model must be loaded in a store");

            if (_disposed)
                throw new Exception("Can not access to a disposed domain.");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the indexes.
        /// </summary>
        /// <value>
        ///  The indexes.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IIndexManager Indexes
        {
            get { return InnerGraph as IIndexManager; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  (Optional) true to throw exception if not exists.
        /// </param>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected TService Resolve<TService>(Func<TService> factory, bool throwExceptionIfNotExists = true) where TService : class
        {
            DebugContract.Requires(factory);
            var svc = factory();
            if (svc == null && throwExceptionIfNotExists)
                throw new Exception(String.Format(ExceptionMessages.ServiceNotFoundForDomainFormat, typeof(TService).Name, Name));

            var service = svc as IDomainService;
            if (service != null)
                service.SetDomain(this);

            return svc;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve hyper graph.
        /// </summary>
        /// <returns>
        ///  An IHyperGraph.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IHyperGraph ResolveHyperGraph()
        {
            return DependencyResolver.Resolve<IHyperGraph>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve command manager.
        /// </summary>
        /// <returns>
        ///  An ICommandManager.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ICommandManager ResolveCommandManager()
        {
            return DependencyResolver.Resolve<ICommandManager>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve model element factory.
        /// </summary>
        /// <returns>
        ///  An IModelElementFactory.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IModelElementFactory ResolveModelElementFactory()
        {
            return DependencyResolver.Resolve<IModelElementFactory>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve identifier generator.
        /// </summary>
        /// <returns>
        ///  An IIdGenerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IIdGenerator ResolveIdGenerator()
        {
            return DependencyResolver.Resolve<IIdGenerator>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve event manager.
        /// </summary>
        /// <returns>
        ///  An IEventManager.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IEventManager ResolveEventManager()
        {
            return DependencyResolver.Resolve<IEventManager>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.Domain.DomainModel and
        ///  optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            var tmp = DomainUnloaded;
            if (tmp != null)
            {
                try
                {
                    tmp(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    var notifier = this.Events as IEventNotifier;
                    if (notifier != null)
                    {
                        var message = new DiagnosticMessage(MessageType.Error, ex.Message, "DomainModelSave", false, null, ex);
                        var list = new ExecutionResult();
                        list.AddMessage(message);
                        notifier.NotifyMessages(list);
                    }
                }
            }

            if (L1Cache != null)
            {
                L1Cache.Dispose();
            }

            var disposable = InnerGraph as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            InnerGraph = null;

            disposable = _commandManager as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            _commandManager = null;

            disposable = _eventManager as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            _eventManager = null;

            disposable = IdGenerator as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            IdGenerator = null;

            disposable = _modelElementFactory as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            _modelElementFactory = null;

            DependencyResolver.Dispose();
            _disposed = true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the initializing action.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void OnInitializing()
        {
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
        ///  Ensures run in session.
        /// </summary>
        /// <returns>
        ///  An ISession.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected ISession EnsuresRunInSession()
        {
            if (Session.Current != null)
                return null;

            return Store.BeginSession();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///  A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///  <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">
        ///  The object to compare with the current object.
        /// </param>
        /// <returns>
        ///  true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public override bool Equals(object obj)
        {
            if (obj is IDomainModel)
                return Name.Equals(((IDomainModel)obj).Name);
            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public override string ToString()
        {
            return Name;
        }

        #region IDomainModel Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier generator.
        /// </summary>
        /// <value>
        ///  The identifier generator.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IIdGenerator IdGenerator
        {
            [DebuggerStepThrough]
            get;
            private set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name.
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Name
        {
            [DebuggerStepThrough]
            get;
            private set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the name of the extension.
        /// </summary>
        /// <value>
        ///  The name of the extension.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string ExtensionName
        {
            [DebuggerStepThrough]
            get;
            protected set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store
        {
            [DebuggerStepThrough]
            get;
            private set;
        }

        bool IUpdatableDomainModel.RemoveEntity(Identity id, ISchemaEntity metadata, bool throwExceptionIfNotExists)
        {
            return RemoveEntityCore(id, metadata, throwExceptionIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the entity core.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metadata">
        ///  the metadata.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  true to throw exception if not exists.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual bool RemoveEntityCore(Identity id, ISchemaEntity metadata, bool throwExceptionIfNotExists)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metadata, "metadata");
            CheckInitialized();

            using (var session = EnsuresRunInSession())
            {
                var r = InnerGraph.RemoveEntity(id, metadata, throwExceptionIfNotExists);
                if (session != null)
                    session.AcceptChanges();
                return r;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IModelElement GetElement(Identity id, ISchemaElement metaclass)
        {
            CheckInitialized();

            if (id == null)
                return null;

            // Dans le cas d'une extension de de domaine, il faut s'assurer de ne conserver dans le cache que les instances
            // d'un type du domaine sous peine d'avoir des casts invalides si le domaine est déchargé
            IModelElement elem = L1Cache.GetElement(id, metaclass);
            return elem;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an element.
        /// </summary>
        /// <typeparam name="TElement">
        ///  Type of the element.
        /// </typeparam>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TElement GetElement<TElement>(Identity id) where TElement : IModelElement
        {
            return (TElement)GetElement(id, Store.GetSchemaElement<TElement>());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an entity.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IModelEntity GetEntity(Identity id, ISchemaEntity metaclass)
        {
            return GetElement(id, metaclass) as IModelEntity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an entity.
        /// </summary>
        /// <typeparam name="TElement">
        ///  Type of the element.
        /// </typeparam>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TElement GetEntity<TElement>(Identity id) where TElement : IModelEntity
        {
            return (TElement)GetEntity(id, Store.GetSchemaEntity<TElement>());
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the entities in this collection.
        /// </summary>
        /// <typeparam name="TElement">
        ///  Type of the element.
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<TElement> GetEntities<TElement>(int skip = 0) where TElement : IModelEntity
        {
            return GetEntities(Store.GetSchemaEntity<TElement>(), skip)
                    .Select(e => (TElement)e);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements in this collection.
        /// </summary>
        /// <param name="metaClass">
        ///  (Optional) the meta class.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<IModelElement> GetElements(ISchemaElement metaClass = null, int skip = 0)
        {
            CheckInitialized();
            return L1Cache.GetElements(metaClass, skip);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the entities in this collection.
        /// </summary>
        /// <param name="metaClass">
        ///  (Optional) the meta class.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<IModelEntity> GetEntities(ISchemaEntity metaClass = null, int skip = 0)
        {
            CheckInitialized();
            return L1Cache.GetEntities(metaClass, skip);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <param name="ownerId">
        ///  The identifier that owns this item.
        /// </param>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual PropertyValue GetPropertyValue(Identity ownerId, ISchemaProperty property)
        {
            Contract.Requires(ownerId, "ownerId");
            Contract.Requires(property, "property");
            CheckInitialized();

            var prop = InnerGraph.GetPropertyValue(ownerId, property);
            if (prop == null || prop.CurrentVersion == 0)
            {
                prop = new PropertyValue { Value = property.DefaultValue, CurrentVersion = 0 };
            }

            return prop;
        }

        PropertyValue IUpdatableDomainModel.SetPropertyValue(IModelElement owner, ISchemaProperty propertyMetadata, object value, long? version)
        {
            return SetPropertyValueCore(owner, propertyMetadata, value, version);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value core.
        /// </summary>
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <param name="propertyMetadata">
        ///  The property metadata.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="version">
        ///  The version.
        /// </param>
        /// <returns>
        ///  A PropertyValue.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual PropertyValue SetPropertyValueCore(IModelElement owner, ISchemaProperty propertyMetadata, object value, long? version)
        {
            Contract.Requires(owner, "owner");
            Contract.Requires(propertyMetadata, "propertyMetadata");

            using (var session = EnsuresRunInSession())
            {
                var r = InnerGraph.SetPropertyValue(owner, propertyMetadata, value, version);
                if (session != null)
                    session.AcceptChanges();
                return r;
            }
        }

        IModelRelationship IUpdatableDomainModel.CreateRelationship(Identity id, ISchemaRelationship relationshipSchema, IModelElement start, Identity endId, ISchemaElement endSchema, IModelRelationship relationship)
        {
            return CreateRelationshipCore(id, relationshipSchema, start, endId, endSchema, relationship);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates relationship core.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="relationshipSchema">
        ///  The relationship schema.
        /// </param>
        /// <param name="start">
        ///  the start.
        /// </param>
        /// <param name="endId">
        ///  The end identifier.
        /// </param>
        /// <param name="endSchema">
        ///  The end schema.
        /// </param>
        /// <param name="relationship">
        ///  The relationship.
        /// </param>
        /// <returns>
        ///  The new relationship core.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IModelRelationship CreateRelationshipCore(Identity id, ISchemaRelationship relationshipSchema, IModelElement start, Identity endId, ISchemaElement endSchema, IModelRelationship relationship)
        {
            Contract.Requires(id, "id");
            Contract.Requires(relationshipSchema, "relationshipSchema");
            Contract.Requires(start, "start");
            Contract.Requires(endId, "endId");
            Contract.Requires(endSchema, "endSchema");

            CheckInitialized();
            using (var session = EnsuresRunInSession())
            {
                relationship = L1Cache.CreateRelationship(id, relationshipSchema, start, endId, endSchema, relationship);

                if (session != null)
                    session.AcceptChanges();
                return relationship;
            }
        }

        bool IUpdatableDomainModel.RemoveRelationship(Identity id, ISchemaRelationship metadata, bool throwExceptionIfNotExists)
        {
            return RemoveRelationshipCore(id, metadata, throwExceptionIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the relationship core.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metadata">
        ///  the metadata.
        /// </param>
        /// <param name="throwExceptionIfNotExists">
        ///  true to throw exception if not exists.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual bool RemoveRelationshipCore(Identity id, ISchemaRelationship metadata, bool throwExceptionIfNotExists)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metadata, "metadata");

            CheckInitialized();
            using (var session = EnsuresRunInSession())
            {
                var r = InnerGraph.RemoveRelationship(id, metadata, throwExceptionIfNotExists);
                if (session != null)
                    session.AcceptChanges();
                return r;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metadata">
        ///  the metadata.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IModelRelationship GetRelationship(Identity id, ISchemaRelationship metadata)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metadata, "metadata");

            CheckInitialized();
            return L1Cache.GetElement(id, metadata) as IModelRelationship;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a relationship.
        /// </summary>
        /// <typeparam name="TRelationship">
        ///  Type of the relationship.
        /// </typeparam>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TRelationship GetRelationship<TRelationship>(Identity id) where TRelationship : IModelRelationship
        {
            return (TRelationship)GetRelationship(id, Store.GetSchemaRelationship<TRelationship>());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships in this collection.
        /// </summary>
        /// <typeparam name="TRelationship">
        ///  Type of the relationship.
        /// </typeparam>
        /// <param name="start">
        ///  (Optional) the start.
        /// </param>
        /// <param name="end">
        ///  (Optional) the end.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<TRelationship> GetRelationships<TRelationship>(IModelElement start = null, IModelElement end = null, int skip = 0) where TRelationship : IModelRelationship
        {
            return GetRelationships(Store.GetSchemaRelationship<TRelationship>(), start, end, skip)
                    .Select(r => (TRelationship)r);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships in this collection.
        /// </summary>
        /// <param name="metadata">
        ///  (Optional) the metadata.
        /// </param>
        /// <param name="start">
        ///  (Optional) the start.
        /// </param>
        /// <param name="end">
        ///  (Optional) the end.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship metadata = null, IModelElement start = null, IModelElement end = null, int skip = 0)
        {
            CheckInitialized();

            return L1Cache.GetRelationships(metadata, start, end, skip);
        }

        #endregion

        internal void OnDomainLoaded()
        {
            var tmp = DomainLoaded;
            if (tmp != null)
                tmp(this, new EventArgs());
        }
    }
}