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
        where TElement : class, IModelElement
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
    ///  Collection of observable model elements.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ObservableModelElementList{T}"/>
    /// <seealso cref="T:System.Collections.Generic.ICollection{T}"/>
    /// <seealso cref="T:System.Collections.Generic.IList{T}"/>
    /// <seealso cref="T:System.Collections.IList"/>
    ///-------------------------------------------------------------------------------------------------
    public class ObservableModelElementCollection<T> : ObservableModelElementList<T>, ICollection<T>, IList<T>, IList where T : class, IModelElement
    {
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
            : base(source, source.DomainModel.Store.GetSchemaRelationship(schemaRelationshipName), opposite, readOnly)
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
        { }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Inserts.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        /// <param name="item">
        ///  The item.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes at described by index.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                throw new NotImplementedException();
            }
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
            get
            {
                return GetItem(index);
            }
            set
            {
                throw new NotImplementedException();
            }
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
        ///  The value to remove.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds an item to the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///  Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="value">
        ///  The value to remove.
        /// </param>
        /// <returns>
        ///  The position into which the new element was inserted, or -1 to indicate that the item was not
        ///  inserted into the collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int Add(object value)
        {
            var mel = value as T;
            if (mel == null)
                throw new ArgumentException("Invalid value type");
            AddInternal(mel);
            return -1;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the first occurrence of a specific object from the
        ///  <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///  Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="value">
        ///  The value to remove.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Remove(object value)
        {
            var mel = value as T;
            if (mel == null)
                throw new ArgumentException("Invalid value type");
            RemoveInternal(mel);
        }
    }


}
