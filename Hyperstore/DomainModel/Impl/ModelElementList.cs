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
    /// <seealso cref="T:Hyperstore.Modeling.ModelElementCollection{TElement}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelElementList<TRelationship, TElement> : ModelElementList<TElement>
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
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelElementList<T> : IDisposable, IEnumerable<T> where T : IModelElement
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

            if (schemaRelationship.Cardinality == Cardinality.OneToOne ||
                (schemaRelationship.Cardinality == Cardinality.OneToMany && opposite) ||
                (schemaRelationship.Cardinality == Cardinality.ManyToOne && !opposite))
                throw new Exception(ExceptionMessages.OnlyOneToManyOrManyToManyAllowedRelationshipsAllowed);

            if (!element.SchemaInfo.IsA(schemaRelationship.Start) && !opposite)
                throw new Exception(ExceptionMessages.InvalidSourceTypeForRelationship);

            if (!element.SchemaInfo.IsA(schemaRelationship.End) && opposite)
                throw new Exception(ExceptionMessages.InvalidEndTypeForRelationship);

            SchemaRelationship = schemaRelationship;
            Source = opposite ? null : element;
            End = opposite ? element : null;
            DomainModel = element.DomainModel;
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
                return DomainModel.GetRelationships(SchemaRelationship, start: Source, end: End, localOnly: true).Count();
            }
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
            foreach (var link in DomainModel.GetRelationships(SchemaRelationship, start: Source, end: End, localOnly: true))
            {
                var mel = (T)(Source != null ? link.End : link.Start);
                if (WhereClause == null || WhereClause(mel))
                    yield return mel;
            }
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