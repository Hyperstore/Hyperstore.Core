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
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling.Commands
{
    /// <summary>
    /// Tracking elements
    /// </summary>
    internal class SessionTrackingData : ISessionTrackingData
    {
        private readonly Dictionary<Identity, TrackedElement> _elements;
        private readonly ISession _session;
        private bool _modelElementsPrepared;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SessionTrackingData(ISession session)
        {
            DebugContract.Requires(session);
            _session = session;
            _elements = new Dictionary<Identity, TrackedElement>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the involved tracking elements.
        /// </summary>
        /// <value>
        ///  The involved elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<TrackedElement> InvolvedTrackedElements
        {
            get { return _elements.Values; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the involved model elements.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <value>
        ///  The involved model elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelElement> InvolvedModelElements
        {
            get
            {
                if (!_modelElementsPrepared)
                    throw new HyperstoreException(ExceptionMessages.InvolvedModelElementsOnlyAvalaibleWhenSessionIsBeingDisposed);

                return _elements.Values.Where(e => e.ModelElement != null)
                        .Select(e => e.ModelElement); // Simule readonly
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets elements by state.
        /// </summary>
        /// <param name="state">
        ///  The state.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the tracking elements by states in
        ///  this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<TrackedElement> GetTrackedElementsByState(TrackingState state)
        {
            return _elements.Values.Where(elem => elem.State == state);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the state of an element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The tracking element state.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TrackingState GetTrackedElementState(Identity id)
        {
            TrackedElement elem;
            if (_elements.TryGetValue(id, out elem))
                return elem.State;
            return TrackingState.Unknown;
        }

        internal void OnEvent(IEvent @event)
        {
            DebugContract.Requires(@event);

            // -----------------------------------------------------------------
            // Add Element
            // -----------------------------------------------------------------
            var addEvent = @event as AddEntityEvent;
            if (addEvent != null)
            {
                var entity = new TrackedElement
                             {
                                 DomainName = @event.Domain,
                                 Extension = @event.ExtensionName,
                                 State = TrackingState.Added,
                                 Id = addEvent.Id,
                                 SchemaId = addEvent.SchemaId,
                                 Version = @event.Version
                             };
                _elements.Add(entity.Id, entity);
                return;
            }

            // -----------------------------------------------------------------
            // Remove Element
            // -----------------------------------------------------------------
            var removeEvent = @event as RemoveEntityEvent;
            if (removeEvent != null)
            {
                TrackedElement entity;
                if (!_elements.TryGetValue(removeEvent.Id, out entity))
                {
                    entity = new TrackedElement
                             {
                                 DomainName = @event.Domain,
                                 Extension = @event.ExtensionName,
                                 State = TrackingState.Removed,
                                 Id = removeEvent.Id,
                                 SchemaId = removeEvent.SchemaId
                             };
                    _elements.Add(entity.Id, entity);
                }
                else
                    entity.State = TrackingState.Removed;

                return;
            }

            // -----------------------------------------------------------------
            // Add metadata element
            // -----------------------------------------------------------------
            var addMetadataEvent = @event as AddSchemaEntityEvent;
            if (addMetadataEvent != null)
            {
                var entity = new TrackedElement
                             {
                                 DomainName = @event.Domain,
                                 Extension = @event.ExtensionName,
                                 State = TrackingState.Added,
                                 Id = addMetadataEvent.Id,
                                 SchemaId = addMetadataEvent.SchemaId,
                                 IsSchema = true,
                                 Version = @event.Version
                             };
                _elements.Add(entity.Id, entity);
                return;
            }

            // -----------------------------------------------------------------
            // Change Element
            // -----------------------------------------------------------------
            var changeEvent = @event as ChangePropertyValueEvent;
            if (changeEvent != null)
            {
                TrackedElement entity;
                if (!_elements.TryGetValue(changeEvent.Id, out entity))
                {
                    entity = new TrackedElement
                             {
                                 DomainName = @event.Domain,
                                 Extension = @event.ExtensionName,
                                 State = TrackingState.Updated,
                                 Id = changeEvent.Id,
                                 SchemaId = changeEvent.SchemaId
                             };
                    _elements.Add(entity.Id, entity);
                }

                var prop = new PropertyValue
                           {
                               Value = changeEvent.GetInternalValue(),
                               CurrentVersion = changeEvent.Version,
                               OldValue = changeEvent.GetInternalOldValue()
                           };
                entity.Properties[changeEvent.PropertyName] = prop;
                entity.Version = Math.Max(entity.Version, changeEvent.Version);
                return;
            }

            // -----------------------------------------------------------------
            // Add relationship
            // -----------------------------------------------------------------
            var addRelationEvent = @event as AddRelationshipEvent;
            if (addRelationEvent != null)
            {
                var entity = new TrackedRelationship
                             {
                                 DomainName = @event.Domain,
                                 Extension = @event.ExtensionName,
                                 State = TrackingState.Added,
                                 Id = addRelationEvent.Id,
                                 SchemaId = addRelationEvent.SchemaId,
                                 StartId = addRelationEvent.StartId,
                                 EndId = addRelationEvent.EndId,
                                 Version = @event.Version
                             };
                _elements.Add(entity.Id, entity);
                return;
            }

            // -----------------------------------------------------------------
            // Remove relationship
            // -----------------------------------------------------------------
            var removeRelationshipEvent = @event as RemoveRelationshipEvent;
            if (removeRelationshipEvent != null)
            {
                TrackedElement entity;
                if (!_elements.TryGetValue(removeRelationshipEvent.Id, out entity))
                {
                    entity = new TrackedRelationship
                             {
                                 DomainName = @event.Domain,
                                 Extension = @event.ExtensionName,
                                 State = TrackingState.Removed,
                                 Id = removeRelationshipEvent.Id,
                                 SchemaId = removeRelationshipEvent.SchemaId,
                                 StartId = removeRelationshipEvent.StartId,
                                 EndId = removeRelationshipEvent.EndId,
                             };
                    _elements.Add(entity.Id, entity);
                }
                else
                    entity.State = TrackingState.Removed;
                return;
            }

            // -----------------------------------------------------------------
            // Add relationship metadata
            // -----------------------------------------------------------------
            var addRelationMetadataEvent = @event as AddSchemaRelationshipEvent;
            if (addRelationMetadataEvent != null)
            {
                var entity = new TrackedRelationship
                             {
                                 DomainName = @event.Domain,
                                 Extension = @event.ExtensionName,
                                 State = TrackingState.Added,
                                 Id = addRelationMetadataEvent.Id,
                                 SchemaId = addRelationMetadataEvent.SchemaId,
                                 StartId = addRelationMetadataEvent.StartId,
                                 EndId = addRelationMetadataEvent.EndId,
                                 IsSchema = true,
                                 Version = @event.Version
                             };
                _elements.Add(entity.Id, entity);
            }
        }

        /// <summary>
        ///     Liste des éléments impactés par les commandes lors de la session. Si plusieurs commandes opérent sur un même
        ///     élément, il ne sera répertorié qu'une fois.
        /// </summary>
        /// <value>
        ///     The involved elements.
        /// </value>
        internal bool PrepareModelElements(bool isAborted, bool schemaLoading)
        {
            if (_modelElementsPrepared)
                return _elements.Count > 0;

            _modelElementsPrepared = true;

            // On est dans le cas d'une session annulée ou lors d'un chargement de metadonnées
            if (schemaLoading)
                return false;

            var set = new HashSet<Identity>();
            var list = _elements.Values.ToList();
            foreach (var element in list)
            {
                if (element is TrackedRelationship)
                {
                    var relationship = element as TrackedRelationship;
                    if (relationship.IsSchema)
                    {
                        //element.ModelElement = session.Store.GetMetaRelationship(element.Id);
                        continue;
                    }

                    if (set.Add(relationship.Id))
                    {
                        if (relationship.State != TrackingState.Removed)
                        {
                            var rel = _session.Store.GetRelationship(relationship.Id);
                            if (rel != null)
                            {
                                if (isAborted)
                                {
                                    ((IDisposable)rel).Dispose();
                                }
                                element.ModelElement = rel;
                            }
                        }

                        if (set.Add(relationship.StartId) && GetTrackedElementState(relationship.StartId) != TrackingState.Removed)
                        {
                            var mel = _session.Store.GetElement(relationship.StartId);
                            if (mel != null) // Au cas ou il a été supprimé
                            {
                                TrackedElement data;
                                if (!_elements.TryGetValue(mel.Id, out data))
                                {
                                    data = new TrackedElement
                                    {
                                        State = TrackingState.Unknown,
                                        Id = mel.Id,
                                        SchemaId = mel.SchemaInfo.Id
                                    };
                                    _elements.Add(mel.Id, data);
                                }
                                data.ModelElement = mel;
                            }
                        }

                        if (set.Add(relationship.EndId) && GetTrackedElementState(relationship.EndId) != TrackingState.Removed)
                        {
                            var mel = _session.Store.GetElement(relationship.EndId);
                            if (mel != null) // Au cas ou il a été supprimé
                            {
                                TrackedElement data;
                                if (!_elements.TryGetValue(mel.Id, out data))
                                {
                                    data = new TrackedElement
                                    {
                                        State = TrackingState.Unknown, // N'a pas de conséquense sur la mise à jour de l'entité
                                        Id = mel.Id,
                                        SchemaId = mel.SchemaInfo.Id
                                    };
                                    _elements.Add(mel.Id, data);
                                }
                                data.ModelElement = mel;
                            }
                        }
                    }
                }
                else
                {
                    // Element
                    if (element.IsSchema)
                    {
                        //element.ModelElement = session.Store.GetMetadata(element.Id);
                    }
                    else
                    {
                        if (set.Add(element.Id))
                        {
                            var mel = _session.Store.GetElement(element.Id);
                            if (mel != null)
                            {
                                if (element.State != TrackingState.Removed)
                                {
                                    element.ModelElement = mel;
                                    if (isAborted)
                                    {
                                        ((IDisposable)mel).Dispose();
                                    }
                                }
                                else if (element.State == TrackingState.Removed)
                                {
                                    ((IDisposable)mel).Dispose();
                                }
                            }
                        }
                    }
                }
            }

            return _elements.Count > 0 && !isAborted;
        }
    }
}