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
    public class ReferenceHandler : IDisposable
    {
        private readonly bool _opposite;
        private ModelElement _owner;
        private Identity _id;
        private ISchemaRelationship _relationshipMetadata;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationship schema.
        /// </summary>
        /// <value>
        ///  The relationship schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship RelationshipSchema { get { return _relationshipMetadata; } }

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
                throw new HyperstoreException(ExceptionMessages.ReferenceHandlerCantBeUsedWithManyToManyRelationship);
            else if (schemaRelationship.Cardinality == Cardinality.ManyToOne && opposite)
                throw new HyperstoreException(ExceptionMessages.ReferenceHandlerCantBeUsedWithManyToManyRelationship);

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
            if (value != null && !value.SchemaInfo.IsA(_opposite ? RelationshipSchema.Start : RelationshipSchema.End))
                throw new HyperstoreException(ExceptionMessages.InvalidValue);
            CheckOwner();
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
            CheckOwner();
            var mel = _owner.GetReference(ref _id, SchemaRelationship, _opposite);
            return mel;
        }

        private void CheckOwner()
        {
            if (_owner == null)
                throw new HyperstoreException("Cannot use disposed element");
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public virtual void Dispose()
        {
            _owner = null;
            _relationshipMetadata = null;
        }
    }
}