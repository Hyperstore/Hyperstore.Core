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
using Hyperstore.Modeling.DomainExtension;
using Hyperstore.Modeling.Ioc;
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
    public class Store : IHyperstore, IExtensionManager
    {
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
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IDomainModelControler<IDomainModel> _domainControler;
        private readonly IDomainModelControler<ISchema> _schemaControler;
        private readonly ILockManager _lockManager;
        private readonly Statistics.Statistics _statistics;
        private bool _disposed;
        private bool _initialized;
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
        ///  Gets the dependency resolver.
        /// </summary>
        /// <value>
        ///  The dependency resolver.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDependencyResolver DependencyResolver
        {
            [DebuggerStepThrough]
            get { return _dependencyResolver; }
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
        /// <param name="options">  (Optional) options for controlling the operation. </param>
        /// <param name="id">       (Optional) The identifier. </param>
        ///-------------------------------------------------------------------------------------------------
        public Store(StoreOptions options = StoreOptions.None, Guid? id = null)
            : this(new DefaultDependencyResolver(), options, id)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        /// <param name="resolver"> The resolver. </param>
        /// <param name="options">  (Optional) options for controlling the operation. </param>
        /// <param name="id">       (Optional) The identifier. </param>
        ///-------------------------------------------------------------------------------------------------
        public Store(IDependencyResolver resolver, StoreOptions options = StoreOptions.None, Guid? id = null)
        {
            Contract.Requires(resolver, "resolver");

            DefaultSessionConfiguration = new SessionConfiguration();
            InitializeSchemaInfoCache();

            _options = options;
            Id = id ?? Guid.NewGuid();
            _statistics = new Statistics.Statistics();
            _dependencyResolver = resolver;

            var r = resolver as IDependencyResolverInternal;
            if (r == null)
                throw new Exception(ExceptionMessages.DependencyResolverMustInheritFromDefaultDependencyResolver);
            r.SetStore(this);

            DefaultSessionConfiguration.IsolationLevel = SessionIsolationLevel.ReadCommitted;
            DefaultSessionConfiguration.SessionTimeout = TimeSpan.FromMinutes(1);

            _domainControler = ((options & StoreOptions.EnableExtensions) == StoreOptions.EnableExtensions) ? (IDomainModelControler<IDomainModel>)new ExtensionModelControler<IDomainModel>() : new DomainModelControler<IDomainModel>();
            _schemaControler = ((options & StoreOptions.EnableExtensions) == StoreOptions.EnableExtensions) ? (IDomainModelControler<ISchema>)new ExtensionModelControler<ISchema>() : new DomainModelControler<ISchema>();

            _lockManager = _dependencyResolver.Resolve<ILockManager>() ?? new LockManager(_dependencyResolver);
            EventBus = _dependencyResolver.Resolve<IEventBus>();
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
                    _traceProvider = DependencyResolver.Resolve<IHyperstoreTrace>() ?? new EmptyHyperstoreTrace();
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetElement<T>(Identity id, bool localOnly = true) where T : IModelElement
        {
            Contract.Requires(id, "id");

            var metaclass = GetSchemaElement<T>(false);
            if (metaclass == null)
                return default(T);

            var mel = GetElement(id, metaclass, localOnly);
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetEntity<T>(Identity id, bool localOnly = true) where T : IModelEntity
        {
            Contract.Requires(id, "id");

            var metaclass = GetSchemaEntity<T>(false);
            if (metaclass == null)
                return default(T);

            var mel = GetEntity(id, metaclass, localOnly);
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelEntity GetEntity(Identity id, ISchemaEntity metaclass, bool localOnly = true)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaclass, "metaclass");

            var domainModel = GetDomainModel(id.DomainModelName);
            if (domainModel == null)
                return null;
            return domainModel.GetEntity(id, metaclass, localOnly);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  the metaclass.
        /// </param>
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement GetElement(Identity id, ISchemaElement metaclass, bool localOnly = true)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaclass, "metaclass");

            var domainModel = GetDomainModel(id.DomainModelName);
            if (domainModel == null)
                return null;
            return domainModel.GetElement(id, metaclass, localOnly);
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetElements<T>(int skip = 0, bool localOnly = true) where T : IModelElement
        {
            var metaclass = GetSchemaElement<T>(false);
            if (metaclass != null)
            {
                foreach (var dm in DomainModels)
                {
                    foreach (var mel in dm.GetElements(metaclass, skip, localOnly))
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
        /// <param name="metaclass">
        ///  (Optional) the metaclass.
        /// </param>
        /// <param name="skip">
        ///  (Optional) the skip.
        /// </param>
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelElement> GetElements(ISchemaElement metaclass = null, int skip = 0, bool localOnly = true)
        {
            foreach (var dm in DomainModels)
            {
                foreach (var mel in dm.GetElements(metaclass, skip, localOnly))
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetEntities<T>(int skip = 0, bool localOnly = true) where T : IModelEntity
        {
            var metaclass = GetSchemaEntity<T>(false);
            if (metaclass != null)
            {
                foreach (var dm in DomainModels)
                {
                    foreach (var mel in dm.GetEntities(metaclass, skip, localOnly))
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelEntity> GetEntities(ISchemaEntity metaclass = null, int skip = 0, bool localOnly = true)
        {
            foreach (var dm in DomainModels)
            {
                foreach (var mel in dm.GetEntities(metaclass, skip, localOnly))
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
        public void UnloadDomainOrExtension(IDomainModel domainOrExtension)
        {
            Contract.Requires(domainOrExtension, "domainOrExtension");
            _domainControler.UnloadDomainExtension(domainOrExtension);
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
        public void UnloadSchemaOrExtension(ISchema schemaOrExtension)
        {
            Contract.Requires(schemaOrExtension, "schemaOrExtension");
            if (schemaOrExtension is PrimitivesSchema)
                throw new Exception("Primitives schema canot be unloaded");

            _schemaControler.UnloadDomainExtension(schemaOrExtension);
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
        public IEnumerable<IDomainModel> DomainModels
        {
            get
            {
                return _domainControler.GetDomainModels();
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
        public IEnumerable<ISchema> Schemas
        {
            get
            {
                return _schemaControler.GetDomainModels();
            }
        }

        List<IEventNotifier> IExtensionManager.GetEventsNotifiers()
        {
            if (_notifiersCache != null)
                return _notifiersCache;

            var domainNotifiers = _domainControler.GetAllDomainModelIncludingExtensions()
                    .Where(domainModel => domainModel.Events is IEventNotifier)
                    .Select(domainmodel => domainmodel.Events as IEventNotifier);

            var schemaNotifiers = _schemaControler.GetAllDomainModelIncludingExtensions()
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

            return _domainControler.GetDomainModel(name) ?? GetSchema(name);
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
            return _schemaControler.GetDomainModel(name);
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship GetRelationship(Identity id, ISchemaRelationship metaRelationship, bool localOnly = true)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaRelationship, "metaRelationship");

            var domainModel = GetDomainModel(id.DomainModelName);
            return domainModel == null ? null : domainModel.GetRelationship(id, metaRelationship, localOnly);
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetRelationship<T>(Identity id, bool localOnly = true) where T : IModelRelationship
        {
            Contract.Requires(id, "id");

            var metaclass = GetSchemaRelationship<T>(false);
            if (metaclass == null)
                return default(T);

            return (T)GetRelationship(id, metaclass, localOnly);
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship metaclass = null, int skip = 0, bool localOnly = true)
        {
            return DomainModels.SelectMany(dm => dm.GetRelationships(metaclass, skip: skip, localOnly: localOnly));
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
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetRelationships<T>(int skip = 0, bool localOnly = true) where T : IModelRelationship
        {
            var metaclass = GetSchemaRelationship<T>(false);
            if (metaclass != null)
                return from dm in DomainModels from rel in dm.GetRelationships(metaclass, skip: skip, localOnly: localOnly) select (T)rel;

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
        /// <param name="domainFactory">
        ///  (Optional) the domain factory.
        /// </param>
        /// <returns>
        ///  The new domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public async Task<IDomainModel> CreateDomainModelAsync(string name, IDomainConfiguration config = null, Func<IDependencyResolver, string, IDomainModel> domainFactory = null)
        {
            Conventions.CheckValidDomainName(name);

            if (GetDomainModel(name) != null)
                throw new Exception("A domain or a schema with the same name already exists in the store.");

            var resolver = DependencyResolver as IDependencyResolverInternal;
            if (resolver == null)
                throw new Exception(ExceptionMessages.DependencyResolverMustInheritFromDefaultDependencyResolver);

            var domainResolver = resolver.CreateDependencyResolver();
            if (config != null)
                config.PrepareDependencyResolver(domainResolver);

            var domainModel = domainFactory != null ? domainFactory(domainResolver, name) : new DomainModel(domainResolver, name);

            // Enregistrement du domaine au niveau du store
            _domainControler.RegisterDomainModel(domainModel);

            // Initialisation du domaine
            domainModel.Configure();

            if (config != null)
            {
                foreach (var r in domainResolver.ResolveAll<RegistrationEventBusSetting>())
                {
                    EventBus.RegisterDomainPolicies(domainModel, r.OutputProperty, r.InputProperty);
                }
            }

            if (config != null)
            {
                var preloadAction = domainResolver.GetSettingValue<Action<IDomainModel>>(DomainConfiguration.PreloadActionKey);
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

            _domainControler.ActivateDomain(domainModel);
            _notifiersCache = null;

            return domainModel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads a schema.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="desc">
        ///  The description.
        /// </param>
        /// <returns>
        ///  The schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public async Task<ISchema> LoadSchemaAsync(ISchemaDefinition desc)
        {
            Contract.Requires(desc, "desc");
            Conventions.CheckValidName(desc.SchemaName, true);

            if (desc.SchemaName == "$" && !(desc is PrimitivesSchemaDefinition))
                throw new Exception("Invalid schema name");

            if (GetDomainModel(desc.SchemaName) != null)
                throw new Exception("A domain with the same name already exists in the store.");

            // On s'assure que le domaine primitif est bien chargé
            await Initialize();

            // Chargement du domaine
            using (var session = BeginSession(new SessionConfiguration { Mode = SessionMode.LoadingSchema }))
            {
                desc.LoadDependentSchemas(this);

                // Création du resolver du domaine à partir du resolver maitre (du store)
                var resolver = DependencyResolver as IDependencyResolverInternal;
                if (resolver == null)
                    throw new Exception(ExceptionMessages.DependencyResolverMustInheritFromDefaultDependencyResolver);

                var domainResolver = resolver.CreateDependencyResolver();

                desc.PrepareDependencyResolver(domainResolver);

                ISchema schema = desc.CreateSchema(domainResolver);

                // Enregistrement du domaine au niveau du store
                _schemaControler.RegisterDomainModel(schema);

                // Initialisation du domaine
                await schema.Initialize(desc);

                foreach (var r in domainResolver.ResolveAll<RegistrationEventBusSetting>())
                {
                    EventBus.RegisterDomainPolicies(schema, r.OutputProperty, r.InputProperty);
                }

                _schemaControler.ActivateDomain(schema);
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

            try
            {
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

                _domainControler.Dispose();

                DependencyResolver.Dispose();
            }
            finally
            {
                if (flag)
                    session.Dispose();
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
            if (!_initialized)
            {
                _initialized = true;

                await LoadSchemaAsync(new PrimitivesSchemaDefinition());
            }
        }

        private void ActivateDomainModel(IDomainModel domainModel)
        {
            // Chargement du méta modéle
            domainModel.Configure();
            _domainControler.ActivateDomain(domainModel);
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