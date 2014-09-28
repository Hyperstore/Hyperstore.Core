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
using System.Linq;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Commands;
using System.Diagnostics;
using System.Threading.Tasks;
using Hyperstore.Modeling.Metadata.Constraints;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  The primitives schema.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISchema"/>
    ///-------------------------------------------------------------------------------------------------
    public class InternalSchema : ISchema
    {
        private readonly IDictionary<Identity, ISchemaInfo> _metadatas = new Dictionary<Identity, ISchemaInfo>(31);
        private readonly IDictionary<string, ISchemaInfo> _metadatasByName = new Dictionary<string, ISchemaInfo>(31);
        private readonly string _name;
        private readonly string _instanceId;
        private readonly IServicesContainer _services;
        private static ISchema _instance;
        internal static ISchema Current { get { return _instance; } }

        internal InternalSchema(IServicesContainer services)
            : this(services, "{2C7C8DA4-5146-4478-9C1E-9DCC2C15481B}", "$$")
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="services">
        ///  The services.
        /// </param>
        /// <param name="instanceId">
        ///  The identifier of the instance.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected internal InternalSchema( IServicesContainer services, string instanceId, string name)
        {
            Store = services.Resolve<IHyperstore>();
            _services = services;
            _instance = this;
            _name = name;
            _instanceId = instanceId;
        }

        IEventManager IDomainModel.Events { get { return null; } }
        IIdGenerator IDomainModel.IdGenerator { get { return null; } }

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
            get { return false; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets domain behaviors.
        /// </summary>
        /// <value>
        ///  The behavior.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public DomainBehavior Behavior { get { return DomainBehavior.Standard; } }

        // Mechant bug sur windowsphone : si on remplace le type du parametre en IMetaElement, l'application crashe sur
        // l'insertion de MetaclassReferencesSuperClass
        internal void RegisterMetadata(IModelElement metaclass)
        {
            DebugContract.Requires(metaclass);
            _metadatas.Add(metaclass.Id, (ISchemaInfo)metaclass);
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
        ISchemaInfo ISchema.GetSchemaInfo(Identity id, bool throwErrorIfNotExists)
        {
            return GetSchemaInfo(id, throwErrorIfNotExists);
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
        ///  true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ISchemaInfo GetSchemaInfo(Identity id, bool throwErrorIfNotExists)
        {
            DebugContract.Requires(id);

            ISchemaInfo metaClass;
            if (_metadatas.TryGetValue(id, out metaClass))
                return metaClass;

            if (throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());

            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a new identifier.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///  Thrown when the requested operation is not supported.
        /// </exception>
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
            throw new NotSupportedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a new identifier.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///  Thrown when the requested operation is not supported.
        /// </exception>
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
            throw new NotSupportedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
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
        ISchemaEntity ISchema.GetSchemaEntity<T>(bool throwErrorIfNotExists) 
        {
            return ((ISchema)this).GetSchemaEntity(new Identity(Name, typeof(T).FullName), throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
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
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity ISchema.GetSchemaEntity(Identity id, bool throwErrorIfNotExists)
        {
            var r = ((ISchema)this).GetSchemaInfo(id, false) as ISchemaEntity;
            if (r == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());
            return r;
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
        ISchemaInfo ISchema.GetSchemaInfo(string name, bool throwErrorIfNotExists)
        {
            DebugContract.RequiresNotEmpty(name);

            ISchemaInfo metaClass;
            if (_metadatasByName.TryGetValue(name, out metaClass))
                return metaClass;

            metaClass = _metadatas.Values.FirstOrDefault(m => m.Name == name);
            if (metaClass != null)
            {
                lock (_metadatasByName)
                {
                    if (!_metadatasByName.ContainsKey(name))
                        _metadatasByName.Add(name, metaClass);
                }
                return metaClass;
            }

            if (throwErrorIfNotExists)
                throw new MetadataNotFoundException(name);

            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
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
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity ISchema.GetSchemaEntity(string name, bool throwErrorIfNotExists)
        {
            var r = ((ISchema)this).GetSchemaInfo(name, false) as ISchemaEntity;
            if (r == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(name);
            return r;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
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
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement ISchema.GetSchemaElement(string name, bool throwErrorIfNotExists)
        {
            var r = ((ISchema)this).GetSchemaInfo(name, false) as ISchemaElement;
            if (r == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(name);
            return r;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
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
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement ISchema.GetSchemaElement(Identity id, bool throwErrorIfNotExists)
        {
            var r = ((ISchema)this).GetSchemaInfo(id, false) as ISchemaElement;
            if (r == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());
            return r;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entities in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema entities in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaEntity> ISchema.GetSchemaEntities()
        {
            return _metadatas.Values.OfType<ISchemaEntity>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema elements in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema elements in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaElement> ISchema.GetSchemaElements()
        {
            return _metadatas.Values.OfType<ISchemaElement>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadatas.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema infos in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaInfo> ISchema.GetSchemaInfos()
        {
            return _metadatas.Values;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationships.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema relationships in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<ISchemaRelationship> ISchema.GetSchemaRelationships()
        {
            return _metadatas.Values.OfType<ISchemaRelationship>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="metaClassId">
        ///  Identifier for the meta class.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship ISchema.GetSchemaRelationship(Identity metaClassId, bool throwErrorIfNotExists)
        {
            DebugContract.Requires(metaClassId);
            return ((ISchema)this).GetSchemaInfo(metaClassId, throwErrorIfNotExists) as ISchemaRelationship;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
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
        ISchemaRelationship ISchema.GetSchemaRelationship(string name, bool throwErrorIfNotExists)
        {
            DebugContract.RequiresNotEmpty(name, "name");
            return ((ISchema)this).GetSchemaInfo(name, throwErrorIfNotExists) as ISchemaRelationship;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
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
        ISchemaRelationship ISchema.GetSchemaRelationship<T>(bool throwErrorIfNotExists )
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model name. This name will be used to create element's identity
        ///  <see cref="Identity" />
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Name
        {
            get { return _name; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the constraints.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <value>
        ///  The constraints.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IConstraintsManager ISchema.Constraints
        {
            get { return null; }
        }

        Task<IDomainScope> IDomainModel.CreateScopeAsync(string extensionName, IDomainConfiguration config)
        {
            throw new NotSupportedException();
        }

        Task<ISchemaExtension> ISchema.LoadSchemaExtension(ISchemaDefinition definition, SchemaConstraintExtensionMode mode)
        {
            throw new NotSupportedException();
        }

        Statistics.DomainStatistics IDomainModel.Statistics
        {
            get { throw new NotSupportedException(); }
        }

        IServicesContainer IDomainModel.Services
        {
            get { return _services; }
        }

        TService IDomainModel.ResolveOrRegisterSingleton<TService>(TService service)
        {
            throw new NotSupportedException();
        }

        System.Threading.Tasks.Task ISchema.Initialize(ISchemaDefinition definition)
        {
            definition.DefineSchema(this);
            OnDomainLoaded();
            return Hyperstore.Modeling.Utils.CompletedTask.Default;
        }

        void IDomainModel.Configure()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the instance.
        /// </summary>
        /// <value>
        ///  The identifier of the instance.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string InstanceId
        {
            get { return _instanceId; }
        }

        string IDomainModel.ExtensionName
        {
            get { return null; }
        }

        private void OnDomainLoaded()
        {
            var tmp = DomainLoaded;
            if (tmp != null)
                tmp(this, new EventArgs());
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

        TService IDomainModel.Resolve<TService>(bool throwExceptionIfNotExists) 
        {
            throw new NotSupportedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public virtual void Dispose()
        {
            var tmp = DomainUnloaded;
            if (tmp != null)
                tmp(this, new EventArgs());
            _services.Dispose();
            Store = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the event dispatcher.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <value>
        ///  The event dispatcher.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Hyperstore.Modeling.Events.IEventDispatcher EventDispatcher { get { return null; } set { throw new NotSupportedException(); } }

        IIndexManager IDomainModel.Indexes
        {
            get { throw new NotSupportedException(); }
        }

        IEnumerable<TElement> IDomainModel.GetEntities<TElement>(int skip)
        {
            yield break;
        }

        IEnumerable<TRelationship> IDomainModel.GetRelationships<TRelationship>(IModelElement start, IModelElement end, int skip)
        {
            yield break;
        }

        TRelationship IDomainModel.GetRelationship<TRelationship>(Identity id)
        {
            throw new NotSupportedException();
        }

        ICommandManager IDomainModel.Commands
        {
            get { throw new NotSupportedException(); }
        }
        IModelElement IDomainModel.GetElement(Identity id, ISchemaElement metaclass)
        {
            DebugContract.Requires(id);
            return ((ISchema)this).GetSchemaInfo(id);
        }

        IModelEntity IDomainModel.GetEntity(Identity id, ISchemaEntity metaclass)
        {
            throw new NotSupportedException();
        }

        TElement IDomainModel.GetElement<TElement>(Identity id)
        {
            throw new NotSupportedException();
        }

        TElement IDomainModel.GetEntity<TElement>(Identity id)
        {
            throw new NotSupportedException();
        }

        IEnumerable<IModelElement> IDomainModel.GetElements(ISchemaElement metadata, int skip)
        {
            yield break;
        }

        IEnumerable<IModelEntity> IDomainModel.GetEntities(ISchemaEntity metadata, int skip)
        {
            yield break;
        }

        IEnumerable<IModelRelationship> IDomainModel.GetRelationships(ISchemaRelationship metadata, IModelElement start, IModelElement end, int skip)
        {
            yield break;
        }

        PropertyValue IDomainModel.GetPropertyValue(Identity ownerId, ISchemaElement ownerSchema, ISchemaProperty propertyMetadata)
        {
            throw new NotSupportedException();
        }

        IModelRelationship IDomainModel.GetRelationship(Identity id, ISchemaRelationship metaclass)
        {
            throw new NotSupportedException();
        }

        Task<int> IDomainModel.LoadAsync(Query query, MergeOption option, Hyperstore.Modeling.Adapters.IGraphAdapter adapter)
        {
            throw new NotSupportedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'domainModel' is same.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <returns>
        ///  true if same, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool SameAs(IDomainModel domainModel)
        {
            return domainModel == this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the traversal.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <value>
        ///  The traversal.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public TraversalBuilder Traversal
        {
            get { throw new NotImplementedException(); }
        }
    }
}