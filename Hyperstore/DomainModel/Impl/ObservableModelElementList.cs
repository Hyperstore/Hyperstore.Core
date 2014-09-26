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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling
{
    public class ObservableModelElementList<T> : AbstractModelElementCollection<T>, INotifyCollectionChanged where T : class, IModelElement
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly object _sync = new object();

        public ObservableModelElementList(IModelElement element, string schemaRelationshipName, bool opposite = false)
            : this(element, element.DomainModel.Store.GetSchemaRelationship(schemaRelationshipName), opposite)
        {
            Contract.Requires(element, "element");
            Contract.RequiresNotEmpty(schemaRelationshipName, "schemaRelationshipName");
        }


        public ObservableModelElementList(IModelElement source, ISchemaRelationship schemaRelationship, bool opposite = false, bool readOnly = false)
            : base(source, schemaRelationship, opposite, readOnly)
        {
            Contract.Requires(source, "source");
            Contract.Requires(schemaRelationship, "schemaRelationship");

            _synchronizationContext = source.DomainModel.Services.Resolve<ISynchronizationContext>();
            if (_synchronizationContext == null)
                throw new Exception("No synchronizationContext founded. You can define a synchronization context in the store with store.Register<ISynchronizationContext>.");

            var query = DomainModel.Events.RelationshipAdded;
            var query2 = DomainModel.Events.RelationshipRemoved;
            var query3 = DomainModel.Events.PropertyChanged;

            query.Subscribe(a => Notify(
                Source != null ? a.Event.End : a.Event.Start,
                Source != null ? a.Event.EndSchema : a.Event.StartSchema,
                Source != null ? a.Event.Start : a.Event.End,
                a.Event.SchemaRelationshipId,
                NotifyCollectionChangedAction.Add));

            query2.Subscribe(a => Notify(
                Source != null ? a.Event.End : a.Event.Start,
                Source != null ? a.Event.EndSchema : a.Event.StartSchema,
                Source != null ? a.Event.Start : a.Event.End,
                a.Event.SchemaRelationshipId,
                NotifyCollectionChangedAction.Remove));

            query3.Subscribe(a => NotifyChange(
                a.Event.ElementId,
                a.Event.SchemaElementId));

        }

        private void NotifyChange(Identity elementId, Identity schemaId)
        {
            var schema = DomainModel.Store.GetSchemaElement(schemaId);
            var valid = Source == null ? schema.IsA(SchemaRelationship.Start) : schema.IsA(SchemaRelationship.End);
            if (valid)
            {
                var index = IndexOfCore(elementId);
                var item = (T)DomainModel.Store.GetElement(elementId, schema);

                if (item == null || (Source != null && !item.DomainModel.SameAs(Source.DomainModel)) || (End != null && !item.DomainModel.SameAs(End.DomainModel)))
                    return;

                if (index >= 0)
                {
                    // Already in the list: check if the query is always valid else remove item from list
                    if (WhereClause != null && !WhereClause((T)item))
                    {
                        _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Remove, index));
                    }
                    return;
                }

                // Not in the list : Is it include in the current relationship ?
                if (End != null && item.DomainModel != End.DomainModel)
                    return; // Inter domain not supported

                if ((Source != null && Source.GetRelationships(SchemaRelationship, item).Any()) || (End != null && End.DomainModel.GetRelationships(SchemaRelationship, item, End).Any()))
                {
                    // Yes, if query is valid, add it to the list
                    if (WhereClause == null || WhereClause((T)item))
                    {
                        index = Count + 1;
                        _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Add, index));
                    }
                }

            }
        }

        private void Notify(Identity elementId, Identity schemaId, Identity startId, Identity rid, NotifyCollectionChangedAction defaultAction)
        {
            if (rid != SchemaRelationship.Id || (Source != null && startId != Source.Id) || (End != null && startId != End.Id))
                return;

            var index = -1;
            if (defaultAction != NotifyCollectionChangedAction.Add)
            {
                index = IndexOfCore(elementId);
                if (index == -1)
                    return;
            }

            var schema = DomainModel.Store.GetSchemaElement(schemaId);
            var item = (T)DomainModel.Store.GetElement(elementId, schema);

            if (item == null || (Source != null && !item.DomainModel.SameAs(Source.DomainModel)) || (End != null && !item.DomainModel.SameAs(End.DomainModel)))
                return;

            if (item != null)
            {
                if ((WhereClause != null && !WhereClause((T)item)) || defaultAction == NotifyCollectionChangedAction.Remove)
                {
                    _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Remove, index));
                }
                else
                {
                    index = Count + 1;
                    _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Add, index));
                }
            }
        }

        private void OnCollectionChanged(object item, NotifyCollectionChangedAction action, int index = -1)
        {
            var tmp = CollectionChanged;
            if (tmp != null)
                tmp(this, new NotifyCollectionChangedEventArgs(action, item, index));
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Occurs when the collection changes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region Ilist<T>
        public int IndexOf(T item)
        {
            return base.IndexOfCore(item.Id);
        }


        #endregion

        #region IList



        public int IndexOf(object value)
        {
            var mel = value as T;
            if (mel == null)
                return -1;
            return base.IndexOfCore(mel.Id);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get { return _sync; }
        }

        public bool Contains(object value)
        {
            var mel = value as T;
            if (mel == null)
                return false;
            return base.Contains(mel);
        }
        #endregion
    }
}