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
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  TODO fusion avec ieventbus ??
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session completed.
        /// </summary>
        /// <value>
        ///  The session completed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<ISessionInformation> SessionCompleting { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session completed.
        /// </summary>
        /// <value>
        ///  The session completed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<ISessionInformation> SessionCompleted { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the custom events.
        /// </summary>
        /// <value>
        ///  The custom events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<IEvent>> CustomEventRaised { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the custom event raising.
        /// </summary>
        /// <value>
        ///  The custom event raising.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<IEvent>> CustomEventRaising { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element added.
        /// </summary>
        /// <value>
        ///  The element added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddEntityEvent>> EntityAdded { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element adding.
        /// </summary>
        /// <value>
        ///  The element adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddEntityEvent>> EntityAdding { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element removed.
        /// </summary>
        /// <value>
        ///  The element removed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<RemoveEntityEvent>> EntityRemoved { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the element removing.
        /// </summary>
        /// <value>
        ///  The element removing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<RemoveEntityEvent>> EntityRemoving { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the attribute changed.
        /// </summary>
        /// <value>
        ///  The attribute changed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<ChangePropertyValueEvent>> PropertyChanged { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property changing.
        /// </summary>
        /// <value>
        ///  The property changing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<ChangePropertyValueEvent>> PropertyChanging { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the attribute removed.
        /// </summary>
        /// <value>
        ///  The attribute removed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<RemovePropertyEvent>> PropertyRemoved { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property removing.
        /// </summary>
        /// <value>
        ///  The property removing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<RemovePropertyEvent>> PropertyRemoving { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship added.
        /// </summary>
        /// <value>
        ///  The relationship added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddRelationshipEvent>> RelationshipAdded { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship adding.
        /// </summary>
        /// <value>
        ///  The relationship adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddRelationshipEvent>> RelationshipAdding { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship removed.
        /// </summary>
        /// <value>
        ///  The relationship removed.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<RemoveRelationshipEvent>> RelationshipRemoved { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship removing.
        /// </summary>
        /// <value>
        ///  The relationship removing.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<RemoveRelationshipEvent>> RelationshipRemoving { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity added.
        /// </summary>
        /// <value>
        ///  The schema entity added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddSchemaEntityEvent>> SchemaEntityAdded { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity adding.
        /// </summary>
        /// <value>
        ///  The schema entity adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddSchemaEntityEvent>> SchemaEntityAdding { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship added.
        /// </summary>
        /// <value>
        ///  The schema relationship added.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddSchemaRelationshipEvent>> SchemaRelationshipAdded { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship adding.
        /// </summary>
        /// <value>
        ///  The schema relationship adding.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<EventContext<AddSchemaRelationshipEvent>> SchemaRelationshipAdding { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the on errors.
        /// </summary>
        /// <value>
        ///  The on errors.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IObservable<ISessionResult> OnErrors { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers for attribute changed event.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IDisposable RegisterForAttributeChangedEvent(IModelElement element);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unregisters for attribute changed event.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void UnregisterForAttributeChangedEvent(IModelElement element);
    }
}