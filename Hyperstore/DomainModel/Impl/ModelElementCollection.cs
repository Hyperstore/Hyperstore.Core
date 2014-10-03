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
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Collection of model elements.
    /// </summary>
    /// <typeparam name="TRelationship">
    ///  Type of the relationship.
    /// </typeparam>
    /// <typeparam name="TElement">
    ///  Type of the element.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElementCollection{TElement}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelElementCollection<TRelationship, TElement> : ModelElementCollection<TElement>
        where TElement : class, IModelElement
        where TRelationship : IModelRelationship
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="element">
        ///  The element.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) true to opposite.
        /// </param>
        /// <param name="readOnly">
        ///  (Optional) true to read only.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ModelElementCollection(IModelElement element, bool opposite = false, bool readOnly = false)
            : base(element, element.DomainModel.Store.GetSchemaRelationship<TRelationship>(), opposite, readOnly)
        {
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Collection of model elements.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.AbstractModelElementCollection{T}"/>
    /// <seealso cref="T:System.Collections.Generic.ICollection{T}"/>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElementList{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelElementCollection<T> : AbstractModelElementCollection<T>, ICollection<T> where T : class, IModelElement
    {
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
        /// <param name="readOnly">
        ///  (Optional) true to read only.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ModelElementCollection(IModelElement element, string schemaRelationshipName, bool opposite = false, bool readOnly = false)
            : base(element, element.DomainModel.Store.GetSchemaRelationship(schemaRelationshipName), opposite, readOnly)
        {
            Contract.Requires(element, "element");
            Contract.RequiresNotEmpty(schemaRelationshipName, "schemaRelationshipName");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="element">
        ///  The element.
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
        public ModelElementCollection(IModelElement element, ISchemaRelationship schemaRelationship, bool opposite = false, bool readOnly = false)
            : base(element, schemaRelationship, opposite, readOnly)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds item.
        /// </summary>
        /// <param name="item">
        ///  The item to add.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Add(T item)
        {
            AddInternal(item);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Clears this instance to its blank/initial state.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Clear()
        {
            ClearInternal();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Copies to.
        /// </summary>
        /// <param name="array">
        ///  The array.
        /// </param>
        /// <param name="arrayIndex">
        ///  Zero-based index of the array.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyToInternal(array, arrayIndex);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the given item.
        /// </summary>
        /// <param name="item">
        ///  The item to remove.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool Remove(T item)
        {
            return RemoveInternal(item);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Collection of abstract model elements.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElementList{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class AbstractModelElementCollection<T> : ModelElementList<T> where T : class, IModelElement
    {
        private readonly bool _readOnly;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="element">
        ///  The element.
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
        protected AbstractModelElementCollection(IModelElement element, ISchemaRelationship schemaRelationship, bool opposite = false, bool readOnly = false)
            : base(element, schemaRelationship, opposite)
        {
            _readOnly = readOnly;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds item.
        /// </summary>
        /// <exception cref="ReadOnlyException">
        ///  Thrown when a Read Only error condition occurs.
        /// </exception>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="item">
        ///  The item to add.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void AddInternal(T item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException();

            if (item == null)
                return;

            var itemMetadata = Source != null ? SchemaRelationship.End : SchemaRelationship.Start;
            if (!((IModelElement)item).SchemaInfo.IsA(itemMetadata))
                throw new TypeMismatchException(ExceptionMessages.InvalidItemType);

            var start = Source ?? item;
            var end = End ?? item;

            var session = EnsuresSession();
            try
            {
                Session.Current.Execute(new AddRelationshipCommand(SchemaRelationship, start, end));
            }
            finally
            {
                if (session != null)
                {
                    session.AcceptChanges();
                    session.Dispose();
                }
            }
        }

        private ISession EnsuresSession()
        {
            if (Session.Current != null)
                return null;
            return DomainModel.Store.BeginSession();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///  true if this instance is read only, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsReadOnly
        {
            get { return _readOnly; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the given item.
        /// </summary>
        /// <exception cref="ReadOnlyException">
        ///  Thrown when a Read Only error condition occurs.
        /// </exception>
        /// <param name="item">
        ///  The item to remove.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected bool RemoveInternal(T item)
        {
            if (IsReadOnly)
                throw new ReadOnlyException();

            if (null == item)
                return false;

            var start = Source ?? item;
            var end = End ?? item;
            var link = start.GetRelationships(SchemaRelationship, end).FirstOrDefault();
            if (link == null)
                return false;

            var cmd = new RemoveRelationshipCommand(link);
            var session = EnsuresSession();
            try
            {
                Session.Current.Execute(cmd);
            }
            finally
            {
                if (session != null)
                {
                    session.AcceptChanges();
                    session.Dispose();
                }
            }
            //  Count--;
            return true; // cmd.Success;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Clears this instance to its blank/initial state.
        /// </summary>
        /// <exception cref="ReadOnlyException">
        ///  Thrown when a Read Only error condition occurs.
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void ClearInternal()
        {
            if (IsReadOnly)
                throw new ReadOnlyException();

            var list = new List<IDomainCommand>();
            foreach (var relationship in DomainModel.GetRelationships(SchemaRelationship, Source, End))
            {
                list.Add(new RemoveRelationshipCommand(relationship));
            }

            var session = EnsuresSession();
            try
            {
                Session.Current.Execute(list.ToArray());
            }
            finally
            {
                if (session != null)
                {
                    session.AcceptChanges();
                    session.Dispose();
                }
            }
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
        public virtual bool Contains(T item)
        {
            return IndexOfCore(item.Id) != -1;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Copies to.
        /// </summary>
        /// <param name="array">
        ///  The array.
        /// </param>
        /// <param name="arrayIndex">
        ///  Zero-based index of the array.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void CopyToInternal(T[] array, int arrayIndex)
        {
            Contract.Requires(array, "array");

            var x = 0;
            var y = 0;
            foreach (var item in this)
            {
                if (x >= arrayIndex)
                    array[y++] = item;
                x++;
            }
        }
    }
}