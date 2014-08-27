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
using System.Threading.Tasks;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.MemoryStore;
using Hyperstore.Modeling.Messaging;
using Hyperstore.Modeling.Scopes;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for hyperstore.
    /// </summary>
    /// <seealso cref="T:IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface IHyperstore : IDisposable
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the lock manager.
        /// </summary>
        /// <value>
        ///  The lock manager.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ILockManager LockManager { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Instance Id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Guid Id { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the dependency resolver.
        /// </summary>
        /// <value>
        ///  The dependency resolver.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IDependencyResolver DependencyResolver { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Domain models list.
        /// </summary>
        /// <value>
        ///  The domain models.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IModelList<IDomainModel> DomainModels { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the schemas. </summary>
        /// <value> The schemas. </value>
        ///-------------------------------------------------------------------------------------------------
        IModelList<ISchema> Schemas { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the default session configuration.
        /// </summary>
        /// <value>
        ///  The default session configuration.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        SessionConfiguration DefaultSessionConfiguration { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the statistics.
        /// </summary>
        /// <value>
        ///  The statistics.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Statistics.Statistics Statistics { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the trace.
        /// </summary>
        /// <value>
        ///  The trace.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IHyperstoreTrace Trace { get; set; }
        /// <summary>
        ///     Occurs when [session created].
        /// </summary>
        event EventHandler<SessionCreatedEventArgs> SessionCreated;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in SessionCompleting events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        event EventHandler<SessionCompletingEventArgs> SessionCompleting;
        /// <summary>
        ///     Occurs when [closed].
        /// </summary>
        event EventHandler<EventArgs> Closed;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Close the store and sends notification to all event subscribers.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Close();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="schemaElementId">
        ///  The schema Element Id.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaInfo GetSchemaInfo(Identity schemaElementId, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <param name="schemaEntityId">
        ///  The schema entity identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity GetSchemaEntity(Identity schemaEntityId, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="schemaElementName">
        ///  Name of the schema element.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaInfo GetSchemaInfo(string schemaElementName, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <param name="schemaElementName">
        ///  Name of the schema element.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity GetSchemaEntity(string schemaElementName, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional)
        ///  if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaInfo GetSchemaInfo<T>(bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaEntity GetSchemaEntity<T>(bool throwErrorIfNotExists = true) where T : IModelEntity;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="relationshipSchemaId">
        ///  The relationship schema identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship GetSchemaRelationship(Identity relationshipSchemaId, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="relationshipSchemaName">
        ///  Name of the relationship schema.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship GetSchemaRelationship(string relationshipSchemaName, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional)
        ///  if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship GetSchemaRelationship<T>(bool throwErrorIfNotExists = true) where T : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get a domain model by its name.
        /// </summary>
        /// <param name="name">
        ///  Domain model name.
        /// </param>
        /// <returns>
        ///  The domain model or null if not exists in the store.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDomainModel GetDomainModel(string name);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets a schema. </summary>
        /// <param name="name"> Domain model name. </param>
        /// <returns>   The schema. </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchema GetSchema(string name);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="entitySchema">
        ///  The entity schema.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelElement GetElement(Identity id, ISchemaElement entitySchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T GetElement<T>(Identity id) where T : IModelElement;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an entity.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="entitySchema">
        ///  The entity schema.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelEntity GetEntity(Identity id, ISchemaEntity entitySchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an entity.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T GetEntity<T>(Identity id) where T : IModelEntity;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement GetSchemaElement<T>(bool throwErrorIfNotExists = true) where T : IModelElement;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <param name="name">
        ///  Domain model name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement GetSchemaElement(string name, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) if set to <c>true</c> [throw error if not exists].
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchemaElement GetSchemaElement(Identity id, bool throwErrorIfNotExists = true);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the session.
        /// </summary>
        /// <param name="sessionConfiguration">
        ///  (Optional)
        /// </param>
        /// <returns>
        ///  An ISession.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISession BeginSession(SessionConfiguration sessionConfiguration = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="relationshipSchema">
        ///  The relationship schema.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelRelationship GetRelationship(Identity id, ISchemaRelationship relationshipSchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T GetRelationship<T>(Identity id) where T : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<T> GetElements<T>(int skip = 0) where T : IModelElement;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <param name="containerSchema">
        ///  (Optional) The entity schema.
        /// </param>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the elements in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelElement> GetElements(ISchemaElement containerSchema = null, int skip = 0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the entities in this collection.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<T> GetEntities<T>(int skip = 0) where T : IModelEntity;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the entities in this collection.
        /// </summary>
        /// <param name="entitySchema">
        ///  (Optional) The entity schema.
        /// </param>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelEntity> GetEntities(ISchemaEntity entitySchema = null, int skip = 0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<T> GetRelationships<T>(int skip = 0) where T : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <param name="relationshipSchema">
        ///  (Optional) The relationship schema.
        /// </param>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship relationshipSchema = null, int skip = 0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event bus.
        /// </summary>
        /// <value>
        ///  The event bus.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEventBus EventBus { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the store options </summary>
        /// <value> The options. </value>
        ///-------------------------------------------------------------------------------------------------
        StoreOptions Options { get; }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for extension manager.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IDomainManager 
    { 
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets events notifiers.
        /// </summary>
        /// <returns>
        ///  The events notifiers.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        List<IEventNotifier> GetEventsNotifiers();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads a schema.
        /// </summary>
        /// <param name="definition">
        ///  The schema definition.
        /// </param>
        /// <param name="parentResolver">
        ///  (Optional) the resolver.
        /// </param>
        /// <returns>
        ///  The schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task<ISchema> LoadSchemaAsync(ISchemaDefinition definition, IDependencyResolver parentResolver = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates domain model.
        /// </summary>
        /// <param name="name">
        ///  Domain model name.
        /// </param>
        /// <param name="config">
        ///  (Optional) the configuration.
        /// </param>
        /// <param name="parentResolver">
        ///  (Optional) the resolver.
        /// </param>
        /// <param name="domainFactory">
        ///  (Optional) the domain factory.
        /// </param>
        /// <returns>
        ///  The new domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task<IDomainModel> CreateDomainModelAsync(string name, IDomainConfiguration config = null, IDependencyResolver parentResolver = null, Func<IDependencyResolver, string, IDomainModel> domainFactory = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unloads the domain extension.
        /// </summary>
        /// <param name="domainOrExtension">
        ///  The domain or extension.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void UnloadDomainOrExtension(IDomainModel domainOrExtension);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Unload schema or extension. </summary>
        /// <param name="schemaOrExtension">    The schema or extension. </param>
        ///-------------------------------------------------------------------------------------------------
        void UnloadSchemaOrExtension(ISchema schemaOrExtension);
    }
}