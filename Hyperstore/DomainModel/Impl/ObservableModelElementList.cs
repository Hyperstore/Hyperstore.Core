using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   List of observable model elements. </summary>
    ///
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    ///-------------------------------------------------------------------------------------------------

    public class ObservableModelElementList<T> : ModelElementList<T>, INotifyCollectionChanged where T : IModelElement
    {
        private ISynchronizationContext _synchronizationContext;
        private readonly List<T> _items;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="element">                  The element. </param>
        /// <param name="schemaRelationshipName">   Name of the schema relationship. </param>
        /// <param name="opposite">                 (Optional) true to opposite. </param>
        ///-------------------------------------------------------------------------------------------------

        public ObservableModelElementList(IModelElement element, string schemaRelationshipName, bool opposite = false)
            : this(element, element.DomainModel.Store.GetSchemaRelationship(schemaRelationshipName), opposite)
        {
            Contract.Requires(element, "element");
            Contract.RequiresNotEmpty(schemaRelationshipName, "schemaRelationshipName");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="source">              The source element. </param>
        /// <param name="schemaRelationship">   The schema relationship. </param>
        /// <param name="opposite">             (Optional) true to opposite. </param>
        ///-------------------------------------------------------------------------------------------------

        public ObservableModelElementList(IModelElement source, ISchemaRelationship schemaRelationship, bool opposite = false) :
            base(source, schemaRelationship, opposite)
        {
            var query = DomainModel.Events.RelationshipAdded;
            var query2 = DomainModel.Events.RelationshipRemoved;
            var query3 = DomainModel.Events.PropertyChanged;

            _synchronizationContext = source.DomainModel.DependencyResolver.Resolve<ISynchronizationContext>();
            if (_synchronizationContext == null)
                throw new Exception("No synchronizationContext founded. You can define a synchronization context in the store with store.Register<ISynchronizationContext>.");

            var initialQuery = DomainModel.GetRelationships(SchemaRelationship, Source, End)
                                          .Select(link => Source != null ? (T)link.End : (T)link.Start);
            if( WhereClause != null)
                initialQuery = initialQuery.Where(WhereClause);

            _items = initialQuery.ToList();

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
            if( valid)
            {
                var index = IndexOfCore(elementId);
                var item = (T)DomainModel.Store.GetElement(elementId, schema);
                if (index >= 0)
                {
                    // Already in the list: check if the query is always valid else remove item from list
                    if (WhereClause != null && !WhereClause((T)item))
                    {
                        lock (_items)
                        {
                            _items.Remove(item);
                        }
                        _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Remove, index));
                    }
                    return;
                }

                // Not in the list : Is it include in the current relationship ?
                if (End != null && item.DomainModel != End.DomainModel )
                    return; // Inter domain not supported

                if((Source != null && Source.GetRelationships(SchemaRelationship, item).Any()) || (End != null && End.DomainModel.GetRelationships(SchemaRelationship, item, End).Any()))
                {
                    // Yes, if query is valid, add it to the list
                    if (WhereClause == null || WhereClause((T)item))
                    {
                        _items.Add(item);
                        index = _items.Count - 1;  
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

            if (item != null)
            {
                if ((WhereClause != null && !WhereClause((T)item)) || defaultAction == NotifyCollectionChangedAction.Remove)
                {
                    lock (_items)
                    {
                        _items.Remove(item);
                    }
                    _synchronizationContext.Send(() => OnCollectionChanged(item, NotifyCollectionChangedAction.Remove, index));
                }
                else
                {
                    lock (_items)
                    {
                        index = IndexOfCore(item.Id);
                        if (index >= 0)
                            return;

                            _items.Add(item);
                            index = _items.Count - 1;                        
                    }
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

        private int IndexOfCore(Identity id)
        {
            lock (_items)
            {
                // TODO optim
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
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _items)
                yield return item;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Occurs when the collection changes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
