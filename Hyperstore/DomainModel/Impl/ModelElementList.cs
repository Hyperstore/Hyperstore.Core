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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hyperstore.Modeling.Traversal;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  List of model elements.
    /// </summary>
    /// <typeparam name="TRelationship">
    ///  Type of the relationship.
    /// </typeparam>
    /// <typeparam name="TElement">
    ///  Type of the element.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElementList{TElement}"/>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElementCollection{TElement}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelElementList<TRelationship, TElement> : ModelElementList<TElement>
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
        ///-------------------------------------------------------------------------------------------------
        public ModelElementList(IModelElement source) //, IGraphTraversalConfiguration query = null)
            : base(source, source.DomainModel.Store.GetSchemaRelationship<TRelationship>()) //, query)
        {
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  List of model elements.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:System.IDisposable"/>
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelElementList<T> : IDisposable, IEnumerable<T> where T : class, IModelElement
    {
        private Func<T, bool> _whereClause;

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
        public ModelElementList(IModelElement element, string schemaRelationshipName, bool opposite = false)
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
        /// <param name="element">
        ///  The element.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) true to opposite.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ModelElementList(IModelElement element, ISchemaRelationship schemaRelationship, bool opposite = false)
        {
            Contract.Requires(schemaRelationship, "schemaRelationship");
            Contract.Requires(element, "element");

            if (schemaRelationship.Cardinality == Cardinality.OneToOne )
                throw new HyperstoreException(ExceptionMessages.OnlyOneToManyOrManyToManyAllowedRelationshipsAllowed);

            if (!opposite && !element.SchemaInfo.IsA(schemaRelationship.Start))
                throw new TypeMismatchException(ExceptionMessages.InvalidSourceTypeForRelationship);

            if (opposite && !element.SchemaInfo.IsA(schemaRelationship.End))
                throw new TypeMismatchException(ExceptionMessages.InvalidEndTypeForRelationship);

            SchemaRelationship = schemaRelationship;
            Source = opposite ? null : element;
            End = opposite ? element : null;
            DomainModel = element.DomainModel;

            Query = from link in DomainModel.GetRelationships(SchemaRelationship, start: Source, end: End)
                    let mel = (T)(Source != null ? link.End : link.Start)
                    where _whereClause == null || WhereClause(mel)
                    select mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the query.
        /// </summary>
        /// <value>
        ///  The query.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IEnumerable<T> Query { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets element schema.
        /// </summary>
        /// <returns>
        ///  The element schema.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected ISchemaElement GetElementSchema()
        {
            return Source == null ? SchemaRelationship.Start : SchemaRelationship.End;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship.
        /// </summary>
        /// <value>
        ///  The schema relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship SchemaRelationship { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the source for the.
        /// </summary>
        /// <value>
        ///  The source.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement Source { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end.
        /// </summary>
        /// <value>
        ///  The end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement End { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the where clause.
        /// </summary>
        /// <value>
        ///  The where clause.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Func<T, bool> WhereClause
        {
            get { return _whereClause; }
            set
            {
                var oldValue = _whereClause;
                _whereClause = value;
                OnWhereClauseChanged(oldValue, value);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of.
        /// </summary>
        /// <value>
        ///  The count.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual int Count
        {
            get
            {
                return Query.Count();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Searches for the first core.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The zero-based index of the found core, or -1 if no match was found.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected int IndexOfCore(Identity id)
        {
            var index = 0;
            foreach (var mel in Query)
            {
                if (mel != null && mel.Id == id)
                    return index;
                index++;
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
        public virtual IEnumerator<T> GetEnumerator()
        {
            return Query.GetEnumerator();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
        protected virtual void OnWhereClauseChanged(Func<T, bool> oldClause, Func<T, bool> newClause)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public virtual void Dispose()
        {
            Source = null;
            DomainModel = null;
            End = null;
            SchemaRelationship = null;
        }
    }
}