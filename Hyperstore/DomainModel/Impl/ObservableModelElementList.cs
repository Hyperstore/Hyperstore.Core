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
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  List of observable model elements.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.AbstractModelElementCollection{T}"/>
    /// <seealso cref="T:System.Collections.Specialized.INotifyCollectionChanged"/>
    ///-------------------------------------------------------------------------------------------------
    public class ObservableModelElementList<T> : AbstractModelElementCollection<T>, INotifyCollectionChanged where T : class, IModelElement
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly object _sync = new object();

        private IDisposable relationshipAddedSubscription;
        private IDisposable relationshipRemovedSubscription;
        private IDisposable propertyChangedSubscription;
        private readonly List<Identity> _items = new List<Identity>();
        private bool _loaded;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        /// <param name="schemaRelationshipName">
        ///  Name of the schema relationship.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) true to opposite.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ObservableModelElementList(IModelElement element, string schemaRelationshipName, bool opposite = false)
            : this(element, element.DomainModel.Store.GetSchemaRelationship(schemaRelationshipName), opposite)
        {
            Contract.Requires(element, "element");
            Contract.RequiresNotEmpty(schemaRelationshipName, "schemaRelationshipName");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="source">
        ///  Source for the.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) true to opposite.
        /// </param>
        /// <param name="readOnly">
        ///  (Optional) true to read only.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
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

            relationshipAddedSubscription = query.Subscribe(a => Notify(
                Source != null ? a.Event.End : a.Event.Start,
                Source != null ? a.Event.EndSchema : a.Event.StartSchema,
                Source != null ? a.Event.Start : a.Event.End,
                a.Event.SchemaRelationshipId,
                NotifyCollectionChangedAction.Add));

            relationshipRemovedSubscription = query2.Subscribe(a => Notify(
                Source != null ? a.Event.End : a.Event.Start,
                Source != null ? a.Event.EndSchema : a.Event.StartSchema,
                Source != null ? a.Event.Start : a.Event.End,
                a.Event.SchemaRelationshipId,
                NotifyCollectionChangedAction.Remove));

            propertyChangedSubscription = query3.Subscribe(a => NotifyPropertyChanged(
                a.Event.ElementId,
                a.Event.SchemaElementId));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public override void Dispose()
        {
            base.Dispose();

            relationshipAddedSubscription.Dispose();
            relationshipRemovedSubscription.Dispose();
            propertyChangedSubscription.Dispose();
        }

        private void NotifyPropertyChanged(Identity elementId, Identity schemaId)
        {
            if (DomainModel == null || WhereClause == null)
                return;

            var schema = DomainModel.Store.GetSchemaElement(schemaId);
            var valid = Source == null ? schema.IsA(SchemaRelationship.Start) : schema.IsA(SchemaRelationship.End);
            if (!valid)
                return;

            var item = (T)DomainModel.Store.GetElement(elementId, schema);
            if (item == null)
                return;

            if (_items.Contains(elementId))
            {
                if (!WhereClause((T)item))
                    NotifyChange(elementId, schemaId, NotifyCollectionChangedAction.Remove, item);
                return;
            }

            if (WhereClause((T)item))
                NotifyChange(elementId, schemaId, NotifyCollectionChangedAction.Add, item);
        }

        private void Notify(Identity elementId, Identity schemaId, Identity startId, Identity rid, NotifyCollectionChangedAction defaultAction)
        {
            if (DomainModel == null || rid != SchemaRelationship.Id || (Source != null && startId != Source.Id) || (End != null && startId != End.Id))
                return;

            NotifyChange(elementId, schemaId, defaultAction, null);
        }

        private void NotifyChange(Identity elementId, Identity schemaId, NotifyCollectionChangedAction defaultAction, IModelElement item)
        {
            int index = _items.IndexOf(elementId);
            if (defaultAction == NotifyCollectionChangedAction.Remove)
            {
                if (index >= 0)
                {
                    if (item == null && DomainModel is Hyperstore.Modeling.Domain.ICacheAccessor)
                    {
                        item = ((Hyperstore.Modeling.Domain.ICacheAccessor)DomainModel).TryGetFromCache(elementId);
                    }
                    _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Remove, index));
                    _items.RemoveAt(index);
                }
                return;
            }

            if (index >= 0)
                return;

            var schema = DomainModel.Store.GetSchemaElement(schemaId);
            if (item == null)
                item = (T)DomainModel.Store.GetElement(elementId, schema);

            if (item == null || WhereClause != null && !WhereClause((T)item))
                return;

            //index = IndexOfCore(elementId);
            //if (index == -1)
            //    return;

            index = _items.Count;
            _items.Add(elementId);
            _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Add, index));
        }

        private void OnCollectionChanged(object item, NotifyCollectionChangedAction action, int index = -1)
        {
            var tmp = CollectionChanged;
            if (tmp != null)
                tmp(this, new NotifyCollectionChangedEventArgs(action, item, index));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an item.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        /// <returns>
        ///  The item.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected T GetItem(int index)
        {
            if (index < 0 || index >= _items.Count)
                return null;

            LoadItems();
            var id = _items[index];
            return Query.FirstOrDefault(e => e.Id == id);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of. 
        /// </summary>
        /// <value>
        ///  The count.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public override int Count
        {
            get
            {
                LoadItems();
                return _items.Count;
            }
        }

        private void LoadItems()
        {
            if (_loaded)
                return;

            lock (_sync)
            {
                if (_loaded)
                    return;

                _items.Clear();
                foreach (var mel in Query)
                {
                    _items.Add(mel.Id);
                }
                _loaded = true;
            }
        }


        struct Iterator<T> : IEnumerator<T> where T : class, IModelElement
        {
            private ObservableModelElementList<T> list;
            private T current;
            private int index;

            public Iterator(ObservableModelElementList<T> list)
            {
                index = 0;
                current = null;
                this.list = list;
            }

            object IEnumerator.Current
            {
                get { return current; }
            }

            bool IEnumerator.MoveNext()
            {
                current = null;
                if (index >= list._items.Count)
                    return false;

                var id = list._items[index];
                current = list.Query.FirstOrDefault(e => e.Id == id);
                index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                index = 0;
            }

            T IEnumerator<T>.Current
            {
                get { return current; }
            }

            void IDisposable.Dispose()
            {
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            LoadItems();
            return new Iterator<T>(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Occurs when the collection changes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region Ilist<T>

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Index of the given value.
        /// </summary>
        /// <param name="item">
        ///  The item.
        /// </param>
        /// <returns>
        ///  An int.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int IndexOf(T item)
        {
            return _items.IndexOf(item.Id);
        }

        #endregion

        #region IList

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Index of the given value.
        /// </summary>
        /// <param name="value">
        ///  The object to test for containment.
        /// </param>
        /// <returns>
        ///  An int.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int IndexOf(object value)
        {
            var mel = value as T;
            if (mel == null)
                return -1;
            return _items.IndexOf(mel.Id);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is fixed size.
        /// </summary>
        /// <value>
        ///  true if this instance is fixed size, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsFixedSize
        {
            get { return false; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is synchronized.
        /// </summary>
        /// <value>
        ///  true if this instance is synchronized, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsSynchronized
        {
            get { return true; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the synchronise root.
        /// </summary>
        /// <value>
        ///  The synchronise root.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object SyncRoot
        {
            get { return _sync; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if this instance contains the given value.
        /// </summary>
        /// <param name="value">
        ///  The object to test for containment.
        /// </param>
        /// <returns>
        ///  true if the object is in this collection, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool Contains(object value)
        {
            return IndexOf(value) != -1;
        }
        #endregion
    }
}