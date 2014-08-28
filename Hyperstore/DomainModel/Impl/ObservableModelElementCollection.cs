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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
//using System.Reactive.Concurrency;
using Hyperstore.Modeling.Events;

#endregion


namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Collection of observable model elements.
    /// </summary>
    /// <typeparam name="TRelationship">
    ///  Type of the relationship.
    /// </typeparam>
    /// <typeparam name="TElement">
    ///  Type of the element.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ObservableModelElementCollection{TElement}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ObservableModelElementCollection<TRelationship, TElement> : ObservableModelElementCollection<TElement>
        where TElement : IModelElement
        where TRelationship : IModelRelationship
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="source">
        ///  Source for the.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) true to opposite.
        /// </param>
        /// <param name="readOnly">
        ///  (Optional) true to read only.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ObservableModelElementCollection(IModelElement source, bool opposite = false, bool readOnly = false)
            : base(source, source.DomainModel.Store.GetSchemaRelationship<TRelationship>(), opposite, readOnly)
        {
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  TODO voir
    ///  https://github.com/reactiveui/ReactiveUI/blob/master/ReactiveUI/ReactiveCollection.cs pour
    ///  finir l'implémentation.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElementCollection{T}"/>
    /// <seealso cref="T:System.Collections.Specialized.INotifyCollectionChanged"/>
    /// <seealso cref="T:System.Collections.Generic.IList{T}"/>
    /// <seealso cref="T:System.Collections.IList"/>
    ///-------------------------------------------------------------------------------------------------
    public class ObservableModelElementCollection<T> : ModelElementCollection<T>, INotifyCollectionChanged, IList<T>, IList where T : IModelElement
    {
        private readonly List<IModelElement> _items;
        private bool _loaded;
        private readonly object _sync = new object();
        private IDisposable _propertyChangedSubscription;
        private readonly ISynchronizationContext _synchronizationContext;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="source">
        ///  Source for the.
        /// </param>
        /// <param name="schemaRelationshipName">
        ///  Name of the schema relationship.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) true to opposite.
        /// </param>
        /// <param name="readOnly">
        ///  (Optional) true to read only.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ObservableModelElementCollection(IModelElement source, string schemaRelationshipName, bool opposite = false, bool readOnly = false)
            : this(source, source.DomainModel.Store.GetSchemaRelationship(schemaRelationshipName), opposite, readOnly)
        {
            Contract.Requires(source, "source");
            Contract.RequiresNotEmpty(schemaRelationshipName, "schemaRelationshipName");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
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
        public ObservableModelElementCollection(IModelElement source, ISchemaRelationship schemaRelationship, bool opposite = false, bool readOnly = false)
            : base(source, schemaRelationship, opposite, readOnly)
        {
            Contract.Requires(source, "source");
            Contract.Requires(schemaRelationship, "schemaRelationship");

            _items = new List<IModelElement>();

            var query = DomainModel.Events.RelationshipAdded;
            var query2 = DomainModel.Events.RelationshipRemoved;

            _synchronizationContext = source.DomainModel.Services.Resolve<ISynchronizationContext>();
            if (_synchronizationContext == null)
                throw new Exception("No synchronizationContext founded. You can define a synchronization context in the store with store.Register<ISynchronizationContext>.");

            query.Subscribe(a => AddItem(a.Event));
            query2.Subscribe(a => RemoveItem(a.Event));
        }

        private void LoadItems()
        {
            if (this.DomainModel.IsDisposed)
                throw new Exception("You can not used element from unloaded domain.");

            if (_loaded)
                return;

            lock (_items)
            {
                if (_loaded)
                    return;

                var query = DomainModel.GetRelationships(SchemaRelationship, Source, End)
                        .Select(link => Source != null ? link.End : link.Start);

                foreach (var mel in query)
                {
                    if (WhereClause == null || WhereClause((T)mel))
                        _items.Add(mel);
                }
                _loaded = true;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an item to the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <returns>
        ///  The position into which the new element was inserted, or -1 to indicate that the item was not
        ///  inserted into the collection,.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int Add(object value)
        {
            base.Add((T)value);
            return -1;//AddItem((T) value);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <returns>
        ///  true if the <see cref="T:System.Object" /> is found in the
        ///  <see cref="T:System.Collections.IList" />; otherwise, false.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool Contains(object value)
        {
            return IndexOf(value) >= 0;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
            //Insert(index, (T) value);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed
        ///  size.
        /// </summary>
        /// <value>
        ///  true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsFixedSize
        {
            get { return false; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the first occurrence of a specific object from the
        ///  <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Remove(object value)
        {
            base.Remove((T)value);
            // On ne met pas à jour directement le tableau d'item, on attend que cela se fasse via
            // l'événement généré si la transaction se termine correctement sinon en cas de rollback
            // le tabealu n'est plus valide
            // RemoveItem(((T)value).Id);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { Insert(index, (T)value); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Copies to.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="array">
        ///  The array.
        /// </param>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
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
        ///  Clears this instance to its blank/initial state.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public override void Clear()
        {
            base.Clear();
            _synchronizationContext.Send(() => OnCollectionChanged(null, NotifyCollectionChangedAction.Reset));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Inserts.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Insert(int index, T value)
        {
            if (value == null)
                return;
            base.Add(value);
            AddItem(value, index);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the entry to access.
        /// </param>
        /// <returns>
        ///  The indexed item.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T this[int index]
        {
            get { LoadItems(); return (T)_items[index]; }
            set { Insert(index, value); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if this instance contains the given item.
        /// </summary>
        /// <param name="item">
        ///  The T to test for containment.
        /// </param>
        /// <returns>
        ///  true if the object is in this collection, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override bool Contains(T item)
        {
            return IndexOfCore(((IModelElement)item).Id) >= 0;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <value>
        ///  The number of elements contained in the <see cref="T:System.Collections.ICollection" />.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public override int Count
        {
            get { LoadItems(); return _items.Count; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes at described by index.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RemoveAt(int index)
        {
            LoadItems();
            var value = _items[index];
            if (value != null)
            {
                base.Remove((T)value);
                RemoveItemAt(IndexOfCore(((IModelElement)value).Id));
            }
        }

        //public new IEnumerator GetEnumerator()
        //{
        //    return base.GetEnumerator();
        //}

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Occurs when the collection changes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the where clause changed action.
        /// </summary>
        /// <param name="oldClause">
        ///  The old clause.
        /// </param>
        /// <param name="newClause">
        ///  The new clause.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected override void OnWhereClauseChanged(Func<T, bool> oldClause, Func<T, bool> newClause)
        {
            if (_propertyChangedSubscription != null)
            {
                _propertyChangedSubscription.Dispose();
                _propertyChangedSubscription = null;
            }

            if (newClause != null)
                _propertyChangedSubscription = DomainModel.Events.PropertyChanged.Subscribe(e => OnPropertyChanged(e.Event));
        }

        private void OnPropertyChanged(ChangePropertyValueEvent evt)
        {
            LoadItems();

            if (WhereClause == null)
                return;

            var pos = IndexOfCore(evt.ElementId);
            if (pos < 0)
                return;

            var item = _items[pos];
            if (item == null)
                return;

            if (WhereClause((T)item) == false)
                RemoveItemAt(pos);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <returns>
        ///  The index of <paramref name="value" /> if found in the list; otherwise, -1.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int IndexOf(object value)
        {
            if (value == null)
                return -1;
            return IndexOfCore(((IModelElement)value).Id);
        }

        private int IndexOfCore(Identity id)
        {
            LoadItems();
            lock (_items)
            {
                // TODO optimisation
                for (var i = 0; i < _items.Count; i++)
                {
                    if (_items[i].Id == id)
                        return i;
                }
            }
            return -1;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Index of the given item.
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
            if (item == null)
                return -1;

            return IndexOfCore(((IModelElement)item).Id);
        }

        private int AddItem(AddRelationshipEvent evt)
        {
            Contract.Requires(evt, "evt");

            IModelElement item = null;
            if (Source != null)
            {
                if (evt.SchemaRelationshipId != SchemaRelationship.Id || (Source != null && evt.Start != Source.Id))
                    return -1;

                var id = evt.End;
                if (IndexOfCore(id) >= 0)
                    return -1;

                item = DomainModel.Store.GetElement(id, SchemaRelationship.End);
                if (item == null || !item.DomainModel.SameAs(Source.DomainModel))
                    return -1;
            }
            else // Opposite
            {
                if (evt.SchemaRelationshipId != SchemaRelationship.Id || (End != null && evt.End != End.Id))
                    return -1;

                var id = evt.Start;
                if (IndexOfCore(id) >= 0)
                    return -1;

                item = DomainModel.Store.GetElement(id, SchemaRelationship.Start);
                if (item == null || !item.DomainModel.SameAs(End.DomainModel))
                    return -1;
            }

            if (End != null && item.DomainModel != End.DomainModel && SchemaRelationship.Cardinality == Cardinality.ManyToMany)
                throw new Exception("Many to many obervableCollection doesn't work on inter domain relationship.");

            return AddItem(item);
        }

        private int AddItem(IModelElement item, int pos = -1)
        {
            DebugContract.Requires(item);

            LoadItems();

            var index = -1;
            if (WhereClause != null && !WhereClause((T)item))
                return -1;

            lock (_items)
            {
                index = IndexOfCore(item.Id);
                if (index < 0)
                {
                    if (pos < 0)
                    {
                        _items.Add(item);
                        index = _items.Count - 1;
                    }
                    else
                    {
                        _items.Insert(pos, item);
                        index = pos;
                    }
                }
            }

            _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Add, index));
            return index;
        }

        private bool RemoveItem(RemoveRelationshipEvent evt)
        {
            Contract.Requires(evt, "evt");
            Identity id;
            if (Source != null)
            {
                if (evt.SchemaRelationshipId != SchemaRelationship.Id || evt.Start != Source.Id)
                    return false;

                id = evt.End;
            }
            else
            {
                if (evt.SchemaRelationshipId != SchemaRelationship.Id || evt.End != End.Id )
                    return false;

                id = evt.Start;
            }

            return RemoveItemAt(IndexOfCore(id));
        }

        private bool RemoveItemAt(int pos)
        {
            LoadItems();

            if (pos < 0)
                return false;

            IModelElement item;
            lock (_items)
            {
                item = _items[pos];
                if (item != null)
                {
                    if( (Source != null && !item.DomainModel.SameAs(Source.DomainModel)) || (End != null && !item.DomainModel.SameAs(End.DomainModel)))
                        return false;

                    if (End != null && item.DomainModel != End.DomainModel && SchemaRelationship.Cardinality == Cardinality.ManyToMany)
                        throw new Exception("Many to many obervableCollection doesn't work on inter domain relationship.");
                    _items.Remove(item);
                }
            }

            if (item == null)
                return false;

            _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Remove, pos));
            return true;
        }

        private void OnCollectionChanged(object item, NotifyCollectionChangedAction action, int index = -1)
        {
            var tmp = CollectionChanged;
            if (tmp != null)
                tmp(this, new NotifyCollectionChangedEventArgs(action, item, index));
        }
    }
}