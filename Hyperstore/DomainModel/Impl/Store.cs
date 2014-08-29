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
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Scopes;
using Hyperstore.Modeling.Container;
using Hyperstore.Modeling.MemoryStore;
using Hyperstore.Modeling.Messaging;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Validations;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A store.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IHyperstore"/>
    /// <seealso cref="T:Hyperstore.Modeling.IExtensionManager"/>
    ///-------------------------------------------------------------------------------------------------
    public class Store : IHyperstore, IDomainManager
    {
        private const string platformTypeName = "Hyperstore.Modeling.Platform.PlatformServicesInstance, Hyperstore.Platform";

        #region Events

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Occurs when [closed].
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler<EventArgs> Closed;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Occurs when [session created].
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler<SessionCreatedEventArgs> SessionCreated;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in SessionCompleting events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler<SessionCompletingEventArgs> SessionCompleting;
        #endregion

        #region Fields
        private List<IEventNotifier> _notifiersCache;
        private readonly IServicesContainer _services;
        private readonly IScopeManager<IDomainModel> _domainControler;
        private readonly IScopeManager<ISchema> _schemaControler;
        private readonly ILockManager _lockManager;
        private Statistics.Statistics _statistics;
        private bool _disposed;
        private int _initialized;
        private readonly StoreOptions _options;
        private Dictionary<string, ISchemaInfo> _schemaInfosCache;
        #endregion

        #region Properties

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets options for controlling the operation. </summary>
        /// <value> The options. </value>
        ///-------------------------------------------------------------------------------------------------
        public StoreOptions Options
        {
            get { return _options; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event bus.
        /// </summary>
        /// <value>
        ///  The event bus.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEventBus EventBus { get; private set; }

        ILockManager IHyperstore.LockManager
        {
            [DebuggerStepThrough]
            get { return _lockManager; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default session configuration.
        /// </summary>
        /// <value>
        ///  The default session configuration.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionConfiguration DefaultSessionConfiguration
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the services container.
        /// </summary>
        /// <value>
        ///  The services container.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IServicesContainer Services
        {
            [DebuggerStepThrough]
            get { return _services; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Instance Id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid Id { get; protected set; }

        #endregion

        #region Constructors

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        /// <param name="services"> The services. </param>
        /// <param name="options">  (Optional) options for controlling the operation. </param>
        /// <param name="id">       (Optional) The identifier. </param>
        ///-------------------------------------------------------------------------------------------------
        internal Store(IServicesContainer services, StoreOptions options = StoreOptions.None, Guid? id = null)
        {
            Contract.Requires(services, "services");

            DefaultSessionConfiguration = new SessionConfiguration();
            InitializeSchemaInfoCache();

            _options = options;
            Id = id ?? Guid.NewGuid();
            _statistics = new Statistics.Statistics();
            _services = services;

            ConfigureServices();

            DefaultSessionConfiguration.IsolationLevel = SessionIsolationLevel.ReadCommitted;
            DefaultSessionConfiguration.SessionTimeout = TimeSpan.FromMinutes(1);

            _domainControler = ((options & StoreOptions.EnableScopings) == StoreOptions.EnableScopings) ? (IScopeManager<IDomainModel>)new ExtendedScopeManager<IDomainModel>(this) : new ScopeManager<IDomainModel>(this);
            _schemaControler = ((options & StoreOptions.EnableScopings) == StoreOptions.EnableScopings) ? (IScopeManager<ISchema>)new ExtendedScopeManager<ISchema>(this) : new ScopeManager<ISchema>(this);

            _lockManager = _services.Resolve<ILockManager>() ?? new LockManager(_services);
            EventBus = _services.Resolve<IEventBus>();
        }

        private void ConfigureServices()
        {
            var platformfactoryType = Type.GetType(platformTypeName, false);
            if (platformfactoryType != null)
            {
                Activator.CreateInstance(platformfactoryType);
            }

            // Store instance
            _services.Register<IHyperstore>(this);
            _services.Register<Hyperstore.Modeling.Statistics.IStatistics>(this.Statistics);
            // Global
            _services.Register<ITransactionManager>(new TransactionManager(_services));
            _services.Register<Hyperstore.Modeling.HyperGraph.IIdGenerator>(r => new GuidIdGenerator());

            // Par domain et par metamodel => Nouvelle instance à chaque fois
            _services.Register<Hyperstore.Modeling.HyperGraph.IHyperGraph>(r => new HyperGraph.HyperGraph(r));
            _services.Register<IEventManager>(r => new Hyperstore.Modeling.Events.EventManager(r));
            _services.Register<IModelElementFactory>(r => Hyperstore.Modeling.Platform.PlatformServices.Current.CreateModelElementFactory());
            _services.Register<ICommandManager>(r => new CommandManager());
            _services.Register<IEventBus>(r => new EventBus(r));
            _services.Register<IConstraintsManager>(r => new ConstraintsManager(r));
            _services.Register<Hyperstore.Modeling.Events.IEventDispatcher>(r => new Hyperstore.Modeling.Events.EventDispatcher(r, true));
            var ctx = System.Threading.SynchronizationContext.Current;
            if (ctx != null)
                _services.Register(ctx);

            _services.Register<ISynchronizationContext>(Hyperstore.Modeling.Platform.PlatformServices.Current.CreateDispatcher());

            // découverte automatique (sans mef) de l'extension rx
            const string typeName = "Hyperstore.ReactiveExtension.SubjectFactory, Hyperstore.ReactiveExtension";
            var rxfactoryType = Type.GetType(typeName, false);
            if (rxfactoryType != null)
            {
                var factory = (ISubjectFactory)Activator.CreateInstance(rxfactoryType);
                _services.Register(factory);
            }
        }

        #endregion

        #region Methods

        private IHyperstoreTrace _traceProvider;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the trace.
        /// </summary>
        /// <value>
        ///  The trace.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstoreTrace Trace
        {
            get
            {
                if (_traceProvider == null)
                {
                    _traceProvider = Services.Resolve<IHyperstoreTrace>() ?? new EmptyHyperstoreTrace();
                }
                return _traceProvider;
            }
            set
            {
                _traceProvider = value;
            }
        }
        //public void ActivateDebug(IDomainModel domainModel)
        //{
        //    System.Diagnostics.Contracts.Contract.Requires(domainModel, "domainModel");
        //    _debugBus = new ModelBus.P2PModelBus(domainModel, ModelBus.EventDirection.Out);
        //    _debugBus.Start();
        //}

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Close the store and sends notification to all event subscribers.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Close()
        {
            Dispose();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///  Thrown when a supplied object has been disposed.
        /// </exception>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="config">
        ///  (Optional) the configuration.
        /// </param>
        /// <returns>
        ///  An ISession.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISession BeginSession(SessionConfiguration config = null)
        {
            if (_disposed)
                throw new ObjectDisposedException("Store");

            if (DefaultSessionConfiguration == null)
                throw new Exception(ExceptionMessages.DefaultConfigurationIsNull);

            var cfg = DefaultSessionConfiguration;
            if (config != null)
                cfg = cfg.Merge(config);

            var session = CreateSessionInternal(cfg);

            if (!session.IsNested)
            {
                OnSessionCreated(session);
                session.Completing += OnSessionCompleting;
            }
            return session;
        }

        void OnSessionCompleting(object sender, SessionCompletingEventArgs e)
        {
            var session = (ISession)sender;
            session.Completing -= OnSessionCompleting;
            var tmp = SessionCompleting;
            if (tmp != null)
            {
                tmp(this, new SessionCompletingEventArgs(session));
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.Store and optionally
        ///  releases the managed resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetElement<T>(Identity id) where T : IModelElement
        {
            Contract.Requires(id, "id");

            var metaclass = GetSchemaElement<T>(false);
            if (metaclass == null)
                return default(T);

            var mel = GetElement(id, metaclass);
            return (T)mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an entity.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetEntity<T>(Identity id) where T : IModelEntity
        {
            Contract.Requires(id, "id");

            var metaclass = GetSchemaEntity<T>(false);
            if (metaclass == null)
                return default(T);

            var mel = GetEntity(id, metaclass);
            return (T)mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an entity.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  the metaclass.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelEntity GetEntity(Identity id, ISchemaEntity metaclass)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaclass, "metaclass");

            var domainModel = GetDomainModel(id.DomainModelName);
            if (domainModel == null)
                return null;
            return domainModel.GetEntity(id, metaclass);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaElement">
        ///  the metaclass.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement GetElement(Identity id, ISchemaElement schemaElement)
        {
            Contract.Requires(id, "id");
            Contract.Requires(schemaElement, "schemaElement");

            var domainModel = GetDomainModel(id.DomainModelName);
            if (domainModel == null)
                return null;
            return domainModel.GetElement(id, schemaElement);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetElements<T>(int skip = 0) where T : IModelElement
        {
            var metaclass = GetSchemaElement<T>(false);
            if (metaclass != null)
            {
                foreach (var dm in DomainModels)
                {
                    foreach (var mel in dm.GetElements(metaclass, skip))
                    {
                        yield return (T)mel;
                    }
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <param name="schemaElement">
        ///  (Optional) the schemaElement.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelElement> GetElements(ISchemaElement schemaElement = null, int skip = 0)
        {
            foreach (var dm in DomainModels)
            {
                foreach (var mel in dm.GetElements(schemaElement, skip))
                {
                    yield return mel;
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the entities in this collection.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetEntities<T>(int skip = 0) where T : IModelEntity
        {
            var metaclass = GetSchemaEntity<T>(false);
            if (metaclass != null)
            {
                foreach (var dm in DomainModels)
                {
                    foreach (var mel in dm.GetEntities(metaclass, skip))
                    {
                        yield return (T)mel;
                    }
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the entities in this collection.
        /// </summary>
        /// <param name="metaclass">
        ///  (Optional) the metaclass.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelEntity> GetEntities(ISchemaEntity metaclass = null, int skip = 0)
        {
            foreach (var dm in DomainModels)
            {
                foreach (var mel in dm.GetEntities(metaclass, skip))
                {
                    yield return mel;
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement GetSchemaElement<T>(bool throwErrorIfNotExists = true) where T : IModelElement
        {
            string fullName = typeof(T).FullName;
            return GetSchemaElement(fullName, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement GetSchemaElement(string name, bool throwErrorIfNotExists = true)
        {
            var result = GetSchemaInfo(name, throwErrorIfNotExists);
            var metaclass = result as ISchemaElement;
            if (metaclass == null && throwErrorIfNotExists)
            {
                if (result != null)
                    throw new Exception(String.Format(ExceptionMessages.ExistsButIsNotASchemaElementFormat, name));

                throw new MetadataNotFoundException(name); // Invalid domain model
            }
            return metaclass;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement GetSchemaElement(Identity id, bool throwErrorIfNotExists = true)
        {
            var result = GetSchemaInfo(id, throwErrorIfNotExists);
            var metaclass = result as ISchemaElement;
            if (metaclass == null && throwErrorIfNotExists)
            {
                if (result != null)
                    throw new Exception(String.Format(ExceptionMessages.ExistsButIsNotASchemaElementFormat, id));

                throw new MetadataNotFoundException(id.ToString()); // Invalid domain model
            }
            return metaclass;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaInfo GetSchemaInfo<T>(bool throwErrorIfNotExists = true)
        {
            return GetSchemaInfo(typeof(T).FullName, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaEntity GetSchemaEntity<T>(bool throwErrorIfNotExists = true) where T : IModelEntity
        {
            var result = GetSchemaInfo<T>(throwErrorIfNotExists);
            var metaclass = result as ISchemaEntity;
            if (metaclass == null && throwErrorIfNotExists)
            {
                if (result != null)
                    throw new Exception(String.Format(ExceptionMessages.ExistsButIsNotASchemaEntityFormat, typeof(T).FullName));
                throw new MetadataNotFoundException(typeof(T).FullName);
            }
            return metaclass;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaEntity GetSchemaEntity(Identity id, bool throwErrorIfNotExists = true)
        {
            var result = GetSchemaInfo(id, throwErrorIfNotExists);
            var metaclass = result as ISchemaEntity;
            if (metaclass == null && throwErrorIfNotExists)
            {
                if (result != null)
                    throw new Exception(String.Format(ExceptionMessages.ExistsButIsNotASchemaEntityFormat, id));
                throw new MetadataNotFoundException(id.ToString());
            }
            return metaclass;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaEntity GetSchemaEntity(string name, bool throwErrorIfNotExists = true)
        {
            var result = GetSchemaInfo(name, throwErrorIfNotExists);
            var metaclass = result as ISchemaEntity;
            if (metaclass == null && throwErrorIfNotExists)
            {
                if (result != null)
                    throw new Exception(String.Format(ExceptionMessages.ExistsButIsNotASchemaEntityFormat, name));
                throw new MetadataNotFoundException(name);
            }
            return metaclass;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaInfo GetSchemaInfo(string name, bool throwErrorIfNotExists = true)
        {
            Contract.RequiresNotEmpty(name, "name");

            ISchemaInfo si;
            if (_schemaInfosCache.TryGetValue(name, out si))
            {
                return si;
            }

            // Parcours les domainModels pour rechercher sur la clé
            foreach (ISchema metaModel in Schemas)
            {
                si = metaModel.GetSchemaInfo(name, false);
                if (si != null)
                {
                    _schemaInfosCache[name] = si;
                    return si;
                }
            }

            if (throwErrorIfNotExists)
                throw new MetadataNotFoundException(name);

            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaInfo GetSchemaInfo(Identity id, bool throwErrorIfNotExists = true)
        {
            Contract.Requires(id, "id");

            ISchemaInfo si;
            if (_schemaInfosCache.TryGetValue(id.ToString(), out si))
            {
                return si;
            }

            var dm = GetSchema(id.DomainModelName);
            if (dm != null)
            {
                si = dm.GetSchemaInfo(id, throwErrorIfNotExists);
                if (si != null)
                {
                    _schemaInfosCache[si.ToString()] = si;
                }
                return si;
            }

            if (throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString()); // Invalid domain model

            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship GetSchemaRelationship<T>(bool throwErrorIfNotExists = true) where T : IModelRelationship
        {
            return GetSchemaRelationship(typeof(T).FullName, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unloads the domain extension.
        /// </summary>
        /// <param name="domainOrExtension">
        ///  The extension.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void IDomainManager.UnloadDomainOrExtension(IDomainModel domainOrExtension)
        {
            Contract.Requires(domainOrExtension, "domainOrExtension");
            _domainControler.UnloadScope(domainOrExtension);
            _notifiersCache = null;
        }

        private void InitializeSchemaInfoCache()
        {
            _schemaInfosCache = new Dictionary<string, ISchemaInfo>(StringComparer.OrdinalIgnoreCase);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unload schema or extension.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="schemaOrExtension">
        ///  The schema or extension.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void IDomainManager.UnloadSchemaOrExtension(ISchema schemaOrExtension)
        {
            Contract.Requires(schemaOrExtension, "schemaOrExtension");
            if (schemaOrExtension is PrimitivesSchema)
                throw new Exception("Primitives schema canot be unloaded");

            _schemaControler.UnloadScope(schemaOrExtension);
            _notifiersCache = null;
            InitializeSchemaInfoCache();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Domain models list.
        /// </summary>
        /// <value>
        ///  The domain models.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelList<IDomainModel> DomainModels
        {
            get
            {
                return _domainControler;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schemas.
        /// </summary>
        /// <value>
        ///  The schemas.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelList<ISchema> Schemas
        {
            get
            {
                return _schemaControler;
            }
        }

        List<IEventNotifier> IDomainManager.GetEventsNotifiers()
        {
            if (_notifiersCache != null)
                return _notifiersCache;

            var domainNotifiers = _domainControler.GetAllScopes()
                    .Where(domainModel => domainModel.Events is IEventNotifier)
                    .Select(domainmodel => domainmodel.Events as IEventNotifier);

            var schemaNotifiers = _schemaControler.GetAllScopes()
                    .Where(domainModel => domainModel.Events is IEventNotifier)
                    .Select(domainmodel => domainmodel.Events as IEventNotifier);

            return _notifiersCache = domainNotifiers.Union(schemaNotifiers).ToList();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get a domain model by its name.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The domain model or null if not exists in the store.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public IDomainModel GetDomainModel(string name)
        {
            Contract.RequiresNotEmpty(name, "name");

            if (name == PrimitivesSchema.DomainModelName)
                return PrimitivesSchema.Current;

            return _domainControler.GetActiveScope(name) ?? GetSchema(name);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a schema.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public ISchema GetSchema(string name)
        {
            Contract.RequiresNotEmpty(name, "name");
            return _schemaControler.GetActiveScope(name);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The meta class.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship GetSchemaRelationship(Identity id, bool throwErrorIfNotExists = true)
        {
            Contract.Requires(id, "id");

            ISchemaInfo si;
            if (_schemaInfosCache.TryGetValue(id.ToString(), out si))
            {
                return (ISchemaRelationship)si;
            }

            // Parcours les domainModels pour rechercher sur la clé
            foreach (ISchema metaModel in Schemas)
            {
                var metadata = metaModel.GetSchemaRelationship(id, false);
                if (metadata != null)
                {
                    _schemaInfosCache[id.ToString()] = metadata;
                    return metadata;
                }
            }

            if (throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());

            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship GetSchemaRelationship(string name, bool throwErrorIfNotExists = true)
        {
            Contract.RequiresNotEmpty(name, "name");

            ISchemaInfo si;
            if (_schemaInfosCache.TryGetValue(name, out si))
            {
                return (ISchemaRelationship)si;
            }

            // Parcours les domainModels pour rechercher sur la clé
            foreach (ISchema metaModel in Schemas)
            {
                var metadata = metaModel.GetSchemaRelationship(name, false);
                if (metadata != null)
                {
                    _schemaInfosCache[name] = metadata;
                    return metadata;
                }
            }

            if (throwErrorIfNotExists)
                throw new MetadataNotFoundException(name);

            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaRelationship">
        ///  The meta relationship.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship GetRelationship(Identity id, ISchemaRelationship metaRelationship)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaRelationship, "metaRelationship");

            var domainModel = GetDomainModel(id.DomainModelName);
            return domainModel == null ? null : domainModel.GetRelationship(id, metaRelationship);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetRelationship<T>(Identity id) where T : IModelRelationship
        {
            Contract.Requires(id, "id");

            var metaclass = GetSchemaRelationship<T>(false);
            if (metaclass == null)
                return default(T);

            return (T)GetRelationship(id, metaclass);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <param name="metaclass">
        ///  (Optional) the metaclass.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship metaclass = null, int skip = 0)
        {
            return DomainModels.SelectMany(dm => dm.GetRelationships(metaclass, skip: skip));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetRelationships<T>(int skip = 0) where T : IModelRelationship
        {
            var metaclass = GetSchemaRelationship<T>(false);
            if (metaclass != null)
                return from dm in DomainModels from rel in dm.GetRelationships(metaclass, skip: skip) select (T)rel;

            return default(IEnumerable<T>);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates domain model.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="config">
        ///  (Optional) the configuration.
        /// </param>
        /// <param name="services">
        ///  The services.
        /// </param>
        /// <param name="domainFactory">
        ///  (Optional) the domain factory.
        /// </param>
        /// <returns>
        ///  The new domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        async Task<IDomainModel> IDomainManager.CreateDomainModelAsync(string name, IDomainConfiguration config, IServicesContainer services, Func<IServicesContainer, string, IDomainModel> domainFactory)
        {
            Conventions.CheckValidDomainName(name);

            if (GetDomainModel(name) != null)
                throw new Exception("A domain with the same name already exists in the store : " + name);

            // On s'assure que le domaine primitif est bien chargé
            await Initialize();

            if (services == null)
            {
                services = Services.NewScope();
                if (config != null)
                    config.PrepareScopedContainer(services);
            }

            var domainModel = domainFactory != null ? domainFactory(services, name) : new DomainModel(services, name);

            // Enregistrement du domaine au niveau du store
            _domainControler.RegisterScope(domainModel);

            // Initialisation du domaine
            domainModel.Configure();

            if (config != null)
            {
                foreach (var r in services.ResolveAll<RegistrationEventBusSetting>())
                {
                    EventBus.RegisterDomainPolicies(domainModel, r.OutputProperty, r.InputProperty);
                }
            }

            if (config != null)
            {
                var preloadAction = services.GetSettingValue<Action<IDomainModel>>(DomainConfiguration.PreloadActionKey);
                if (preloadAction != null)
                {
                    await Task.Run(() =>
                        {
                            using (var session = this.BeginSession(new SessionConfiguration { Mode = SessionMode.Loading }))
                            {
                                preloadAction(domainModel);
                                session.AcceptChanges();
                            }
                        });
                }
            }

            _domainControler.ActivateScope(domainModel);
            _notifiersCache = null;

            return domainModel;
        }

        async Task<ISchema> IDomainManager.LoadSchemaAsync(ISchemaDefinition desc, IServicesContainer parentContainer)
        {
            Contract.Requires(desc, "desc");
            Conventions.CheckValidName(desc.SchemaName, true);

            // On s'assure que le domaine primitif est bien chargé
            await Initialize();

            if (desc.SchemaName == PrimitivesSchema.DomainModelName && !(desc is PrimitivesSchemaDefinition))
                throw new Exception("Invalid schema name");

            if (desc.SchemaName != PrimitivesSchema.DomainModelName && !(desc is ExtensionSchemaDefinition) && GetDomainModel(desc.SchemaName) != null)
                throw new Exception("A domain or schema with the same name already exists in the store : " + desc.SchemaName);

            // Chargement du domaine
            using (var session = BeginSession(new SessionConfiguration { Mode = SessionMode.LoadingSchema }))
            {
                desc.LoadDependentSchemas(this);

                // Création du services du domaine à partir du services maitre (du store)
                var domainContainer = (parentContainer ?? Services).NewScope();

                desc.PrepareScopedContainer(domainContainer);

                var schema = desc.CreateSchema(domainContainer);

                // Enregistrement du domaine au niveau du store
                _schemaControler.RegisterScope(schema);

                // Initialisation du domaine
                await schema.Initialize(desc);

                foreach (var r in domainContainer.ResolveAll<RegistrationEventBusSetting>())
                {
                    EventBus.RegisterDomainPolicies(schema, r.OutputProperty, r.InputProperty);
                }

                _schemaControler.ActivateScope(schema);
                _notifiersCache = null;
                session.AcceptChanges();

                return schema;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates session internal.
        /// </summary>
        /// <param name="cfg">
        ///  The configuration.
        /// </param>
        /// <returns>
        ///  The new session internal.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ISession CreateSessionInternal(SessionConfiguration cfg)
        {
            var session = new Session(this, cfg);
            return session;
        }

        private void OnSessionCreated(ISession session)
        {
            DebugContract.Requires(session, "session");
            _domainControler.OnSessionCreated(session);

            var tmp = SessionCreated;
            if (tmp != null)
                tmp(this, new SessionCreatedEventArgs(session));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Finaliser.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        ~Store()
        {
            Dispose(false);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.Store and optionally
        ///  releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            var session = Session.Current;
            var flag = session == null;
            try
            {
                if (flag)
                {
                    session = BeginSession(new SessionConfiguration
                                           {
                                               Readonly = true
                                           });
                }

                _disposed = true; // Il n'est plus possible de créer des sessions

                var disposable = EventBus as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                EventBus = null;

                _statistics = null;

                // Envoi de l'événement OnCompleted à tous les observeurs
                NotifyEnd();
                var tmp = Closed;
                if (tmp != null)
                {
                    try
                    {
                        tmp(this, new EventArgs());
                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                if (flag && session != null)
                    session.Dispose();

                _domainControler.Dispose();
                _schemaControler.Dispose();
                Services.Dispose();

                _schemaInfosCache.Clear();
                _notifiersCache.Clear();

                CodeMarker.Dispose();
            }
        }

        private ISession EnsuresRunInSession()
        {
            if (Session.Current != null)
                return null;

            return BeginSession(new SessionConfiguration
                                {
                                    Readonly = true
                                });
        }

        private void NotifyEnd()
        {
            foreach (var domainModel in DomainModels)
            {
                var notifier = domainModel.Events as IEventNotifier;
                if (notifier != null)
                    notifier.NotifyEnd();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual async Task Initialize()
        {
            var manager = this as IDomainManager;
            if (manager == null)
                throw new Exception("Current store must implement IDomainManager");

            if( System.Threading.Interlocked.CompareExchange( ref _initialized, 1, 0) == 0)
            {
                await manager.LoadSchemaAsync(new PrimitivesSchemaDefinition());
            }
        }

        private void ActivateDomainModel(IDomainModel domainModel)
        {
            // Chargement du méta modéle
            domainModel.Configure();
            _domainControler.ActivateScope(domainModel);
        }

        #endregion

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the statistics.
        /// </summary>
        /// <value>
        ///  The statistics.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Statistics.Statistics Statistics
        {
            get { return _statistics; }
        }
    }
}