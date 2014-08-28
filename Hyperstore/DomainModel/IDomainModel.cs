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
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Messaging;
using Hyperstore.Modeling.Statistics;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for domain model.
    /// </summary>
    /// <seealso cref="T:IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface IDomainModel : IDisposable
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the id generator.
        /// </summary>
        /// <value>
        ///  The id generator.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Hyperstore.Modeling.HyperGraph.IIdGenerator IdGenerator { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event manager.
        /// </summary>
        /// <value>
        ///  The events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEventManager Events { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the instance id.
        /// </summary>
        /// <value>
        ///  The instance id.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string InstanceId { get; }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model name. This name will be used to create element's identity
        ///  <see cref="Identity" />
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string Name { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  If the domain is an extension, gets its name
        /// </summary>
        /// <value>
        ///  The extension name or null if it's not an extension.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string ExtensionName { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IHyperstore Store { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the command manager.
        /// </summary>
        /// <value>
        ///  The command manager.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ICommandManager Commands { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the index manager.
        /// </summary>
        /// <value>
        ///  The index manager.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IIndexManager Indexes { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the statistics.
        /// </summary>
        /// <value>
        ///  The statistics.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        DomainStatistics Statistics { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the services container.
        /// </summary>
        /// <value>
        ///  The services container.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IServicesContainer Services { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves or register a scoped service.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <param name="service">
        ///  The service.
        /// </param>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TService ResolveOrRegisterSingleton<TService>(TService service) where TService : class;

        /// <summary>
        ///     Occurs when [on domain loaded].
        /// </summary>
        event EventHandler DomainLoaded;

        /// <summary>
        ///     Occurs when [domain unloaded].
        /// </summary>
        event EventHandler DomainUnloaded;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an element by identity.
        /// </summary>
        /// <param name="id">
        ///  The id of the element.
        /// </param>
        /// <param name="containerSchema">
        ///  The container schema.
        /// </param>
        /// <returns>
        ///  Null if the element doesn't exist in the domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelElement GetElement(Identity id, ISchemaElement containerSchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element.
        /// </summary>
        /// <typeparam name="TElement">
        ///  The type of the element.
        /// </typeparam>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TElement GetElement<TElement>(Identity id) where TElement : IModelElement;

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
        /// <typeparam name="TElement">
        ///  Type of the element.
        /// </typeparam>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TElement GetEntity<TElement>(Identity id) where TElement : IModelEntity;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all element or relationships.
        /// </summary>
        /// <param name="containerSchema">
        ///  (Optional) The container schema.
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
        ///  Gets all elements by metaclass.
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
        ///  Gets the elements.
        /// </summary>
        /// <typeparam name="TElement">
        ///  The type of the element.
        /// </typeparam>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the entities in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<TElement> GetEntities<TElement>(int skip = 0) where TElement : IModelEntity;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <param name="ownerId">
        ///  The identifier that owns this item.
        /// </param>
        /// <param name="ownerSchema">
        ///  The schema that owns this item.
        /// </param>
        /// <param name="propertySchema">
        ///  The property schema.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        PropertyValue GetPropertyValue(Identity ownerId, ISchemaElement ownerSchema, ISchemaProperty propertySchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <param name="id">
        ///  The id.
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
        /// <typeparam name="TRelationship">
        ///  The type of the relationship.
        /// </typeparam>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TRelationship GetRelationship<TRelationship>(Identity id) where TRelationship : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <param name="relationshipSchema">
        ///  (Optional) The relationship schema.
        /// </param>
        /// <param name="start">
        ///  (Optional) The start.
        /// </param>
        /// <param name="end">
        ///  (Optional) The end.
        /// </param>
        /// <param name="skip">
        ///  (Optional) The skip.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship relationshipSchema = null, IModelElement start = null, IModelElement end = null, int skip = 0);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <typeparam name="TRelationship">
        ///  The type of the relationship.
        /// </typeparam>
        /// <param name="start">
        ///  (Optional) The start.
        /// </param>
        /// <param name="end">
        ///  (Optional) The end.
        /// </param>
        /// <param name="skip">
        ///  (Optional)
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<TRelationship> GetRelationships<TRelationship>(IModelElement start = null, IModelElement end = null, int skip = 0) where TRelationship : IModelRelationship;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolves this instance.
        /// </summary>
        /// <typeparam name="TService">
        ///  The type of the service.
        /// </typeparam>
        /// <param name="throwExceptionIfNotExists">
        ///  (Optional) true to throw exception if not exists.
        /// </param>
        /// <returns>
        ///  A TService.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TService Resolve<TService>(bool throwExceptionIfNotExists = true) where TService : class;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Configures this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Configure();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the event dispatcher.
        /// </summary>
        /// <value>
        ///  The event dispatcher.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Hyperstore.Modeling.Events.IEventDispatcher EventDispatcher { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Loads an extension. </summary>
        /// <param name="scopeName">    The scope name or null if it's not an extension. </param>
        /// <param name="configuration">    (Optional) the configuration. </param>
        /// <returns>   The new scope. </returns>
        ///-------------------------------------------------------------------------------------------------
        Task<IDomainScope> CreateScopeAsync(string scopeName, IDomainConfiguration configuration = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates a new identifier.
        /// </summary>
        /// <param name="key">
        ///  (Optional) the key.
        /// </param>
        /// <param name="schemaElement">
        ///  (Optional) the schema element.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Identity CreateId(string key = null, ISchemaElement schemaElement = null);

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
        Identity CreateId(long key, ISchemaElement schemaElement = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///  true if this instance is disposed, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsDisposed { get; }

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
        bool SameAs(IDomainModel domainModel);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <param name="option">
        ///  The option.
        /// </param>
        /// <param name="adapter">
        ///  The adapter.
        /// </param>
        /// <returns>
        ///  Number of nodes loaded
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Task<int> LoadAsync(Query query = null, MergeOption option = MergeOption.OverwriteChanges, Hyperstore.Modeling.Adapters.IGraphAdapter adapter = null);
    }
}