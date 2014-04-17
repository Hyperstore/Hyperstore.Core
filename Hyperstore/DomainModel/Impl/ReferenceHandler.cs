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

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A reference handler.
    /// </summary>
    /// <typeparam name="TMetaRelationship">
    ///  The type of the meta relationship.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ReferenceHandler"/>
    ///-------------------------------------------------------------------------------------------------
    public class ReferenceHandler<TMetaRelationship> : ReferenceHandler where TMetaRelationship : IModelRelationship
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="ReferenceHandler{TMetaRelationship}" /> class.
        /// </summary>
        /// <param name="mel">
        ///  The mel.
        /// </param>
        /// <param name="opposite">
        ///  (Optional)
        ///  if set to <c>true</c> [opposite].
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ReferenceHandler(ModelElement mel, bool opposite = false)
            : base(mel, mel.Store.GetSchemaRelationship<TMetaRelationship>(), opposite)
        {
            Contract.Requires(mel, "mel");
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Gestionnaire des références.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class ReferenceHandler
    {
        private readonly bool _opposite;
        private readonly ModelElement _owner;
        private Identity _id;
        private ISchemaRelationship _relationshipMetadata;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="ReferenceHandler" /> class.
        /// </summary>
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <param name="schemaRelationshipName">
        ///  Name of the schema relationship.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) if set to <c>true</c> [opposite].
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ReferenceHandler(ModelElement owner, string schemaRelationshipName, bool opposite = false)
            : this(owner, owner.Store.GetSchemaRelationship(schemaRelationshipName), opposite)
        {
            Contract.Requires(owner, "owner");
            Contract.RequiresNotEmpty(schemaRelationshipName, "schemaRelationshipName");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="ReferenceHandler" /> class.
        /// </summary>
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <param name="schemaRelationship">
        ///  The relationship metadata.
        /// </param>
        /// <param name="opposite">
        ///  (Optional) if set to <c>true</c> [opposite].
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ReferenceHandler(ModelElement owner, ISchemaRelationship schemaRelationship, bool opposite = false)
        {
            Contract.Requires(owner, "owner");
            Contract.Requires(schemaRelationship, "schemaRelationship");

            if (schemaRelationship.Cardinality == Cardinality.ManyToMany)
                throw new System.Exception(ExceptionMessages.ReferenceHandlerCantBeUsedWithManyToManyRelationship);
            else if( schemaRelationship.Cardinality == Cardinality.ManyToOne && opposite)
                throw new System.Exception(ExceptionMessages.ReferenceHandlerCantBeUsedWithManyToManyRelationship);

            _owner = owner;
            SchemaRelationship = schemaRelationship;
            _opposite = opposite;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the schema relationship.
        /// </summary>
        /// <value>
        ///  The schema relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected ISchemaRelationship SchemaRelationship
        {
            get { return _relationshipMetadata; }
            set { _relationshipMetadata = value; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship.
        /// </summary>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship GetRelationship()
        {
            if (_id == null)
                GetReference(); // Recherche id - Normal qu'il n'y ait pas d'affectation
            return _id != null ? _owner.Store.GetRelationship(_id, _relationshipMetadata) : null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets the reference.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetReference(IModelElement value)
        {
            _owner.SetReference(ref _id, SchemaRelationship, value, _opposite);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the reference.
        /// </summary>
        /// <returns>
        ///  The reference.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement GetReference()
        {
            if (((IModelElement)_owner).Status == ModelElementStatus.Disposed)
                return null;

            var mel = _owner.GetReference(ref _id, SchemaRelationship, _opposite);
            return mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the reference.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <returns>
        ///  The reference.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetReference<T>() where T : IModelElement
        {
            return (T)GetReference();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the references.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the references in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelElement> GetReferences()
        {
            return _owner.GetRelationships(SchemaRelationship);
        }
    }
}