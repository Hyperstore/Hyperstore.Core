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
                    throw new Exception(ExceptionMessages.InvolvedModelElementsOnlyAvalaibleWhenSessionIsBeingDisposed);

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
                                 State = TrackingState.Added,
                                 Id = addEvent.Id,
                                 SchemaId = addEvent.SchemaEntityId,
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
                                 State = TrackingState.Removed,
                                 Id = removeEvent.Id,
                                 SchemaId = removeEvent.SchemaEntityId
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
                                 State = TrackingState.Added,
                                 Id = addMetadataEvent.Id,
                                 SchemaId = addMetadataEvent.SchemaEntityId,
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
                if (!_elements.TryGetValue(changeEvent.ElementId, out entity))
                {
                    entity = new TrackedElement
                             {
                                 State = TrackingState.Updated,
                                 Id = changeEvent.ElementId,
                                 SchemaId = changeEvent.SchemaElementId
                             };
                    _elements.Add(entity.Id, entity);
                }

                var prop = new PropertyValue
                           {
                               Value = changeEvent.InternalValue,
                               CurrentVersion = changeEvent.Version,
                               OldValue = changeEvent.InternalOldValue
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
                                 State = TrackingState.Added,
                                 Id = addRelationEvent.RelationshipId,
                                 SchemaId = addRelationEvent.SchemaRelationshipId,
                                 StartId = addRelationEvent.Start,
                                 StartSchemaId = addRelationEvent.StartSchema,
                                 EndId = addRelationEvent.End,
                                 EndSchemaId = addRelationEvent.EndSchema,
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
                if (!_elements.TryGetValue(removeRelationshipEvent.RelationshipId, out entity))
                {
                    entity = new TrackedRelationship
                             {
                                 State = TrackingState.Removed,
                                 Id = removeRelationshipEvent.RelationshipId,
                                 SchemaId = removeRelationshipEvent.SchemaRelationshipId,
                                 StartId = removeRelationshipEvent.Start,
                                 StartSchemaId = removeRelationshipEvent.StartSchema,
                                 EndId = removeRelationshipEvent.End,
                                 EndSchemaId = removeRelationshipEvent.EndSchema
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
                                 State = TrackingState.Added,
                                 Id = addRelationMetadataEvent.Id,
                                 SchemaId = addRelationMetadataEvent.SchemaRelationshipId,
                                 StartId = addRelationMetadataEvent.Start,
                                 StartSchemaId = addRelationMetadataEvent.StartSchema,
                                 EndId = addRelationMetadataEvent.End,
                                 EndSchemaId = addRelationMetadataEvent.EndSchema,
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
                            var metadata = _session.Store.GetSchemaRelationship(relationship.SchemaId);
                            var rel = _session.Store.GetRelationship(relationship.Id, metadata);
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
                            var mel = _session.Store.GetElement(relationship.StartId, _session.Store.GetSchemaElement(relationship.StartSchemaId));
                            if (mel != null) // Au cas ou il a été supprimé
                            {
                                TrackedElement data;
                                if( !_elements.TryGetValue(mel.Id, out data))
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
                            var mel = _session.Store.GetElement(relationship.EndId, _session.Store.GetSchemaElement(relationship.EndSchemaId));
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
                            var metadata = _session.Store.GetSchemaElement(element.SchemaId);
                            var mel = _session.Store.GetElement(element.Id, metadata);
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
                                else if( element.State == TrackingState.Removed)
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