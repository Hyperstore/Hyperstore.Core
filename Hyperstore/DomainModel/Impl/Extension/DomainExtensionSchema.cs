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
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Validations;

#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal class DomainExtensionSchema : DomainSchema, IExtension
    {
        private readonly ISchema _extendedMetaModel;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="extendedMetaModel">
        ///  The extended meta model.
        /// </param>
        /// <param name="dependencyResolver">
        ///  The dependency resolver.
        /// </param>
        /// <param name="constraints">
        ///  (Optional)
        ///  The constraints.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainExtensionSchema(ISchema extendedMetaModel, IDependencyResolver dependencyResolver, DomainBehavior behavior=DomainBehavior.None, IConstraintsManager constraints=null) 
            : base(extendedMetaModel.Name, dependencyResolver, behavior, constraints)
        {
            DebugContract.Requires(extendedMetaModel);
            DebugContract.Requires(dependencyResolver);
            DebugContract.Requires(constraints);

            _extendedMetaModel = extendedMetaModel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaElement GetSchemaElement(Identity id, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaElement(id, false) ?? _extendedMetaModel.GetSchemaElement(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaElement GetSchemaElement(string name, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaElement(name, false) ?? _extendedMetaModel.GetSchemaElement(name, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaEntity GetSchemaEntity(Identity id, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaEntity(id, false) ?? _extendedMetaModel.GetSchemaEntity(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaEntity GetSchemaEntity(string name, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaEntity(name, false) ?? _extendedMetaModel.GetSchemaEntity(name, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaInfo GetSchemaInfo(Identity id, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaInfo(id, false) ?? _extendedMetaModel.GetSchemaInfo(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaInfo GetSchemaInfo(string name, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaInfo(name, false) ?? _extendedMetaModel.GetSchemaInfo(name, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema elements in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema elements in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IEnumerable<ISchemaElement> GetSchemaElements()
        {
            return base.GetSchemaElements()
                    .Union(_extendedMetaModel.GetSchemaElements());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entities in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema entities in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IEnumerable<ISchemaEntity> GetSchemaEntities()
        {
            return base.GetSchemaEntities()
                    .Union(_extendedMetaModel.GetSchemaEntities());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadatas.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema infos in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IEnumerable<ISchemaInfo> GetSchemaInfos()
        {
            return base.GetSchemaInfos()
                    .Union(_extendedMetaModel.GetSchemaInfos());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaRelationship GetSchemaRelationship(Identity id, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaRelationship(id, false) ?? _extendedMetaModel.GetSchemaRelationship(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ISchemaRelationship GetSchemaRelationship(string name, bool throwErrorIfNotExists = true)
        {
            return base.GetSchemaRelationship(name, false) ?? _extendedMetaModel.GetSchemaRelationship(name, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationships.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema relationships in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IEnumerable<ISchemaRelationship> GetSchemaRelationships()
        {
            return base.GetSchemaRelationships()
                    .Union(_extendedMetaModel.GetSchemaRelationships());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IModelElement GetElement(Identity id, ISchemaElement metaclass, bool localOnly = true)
        {
            return base.GetElement(id, metaclass, localOnly) ?? _extendedMetaModel.GetElement(id, metaclass, localOnly);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an entity.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IModelEntity GetEntity(Identity id, ISchemaEntity metaclass, bool localOnly = true)
        {
            return base.GetEntity(id, metaclass, localOnly) ?? _extendedMetaModel.GetEntity(id, metaclass, localOnly);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  the metadata.
        /// </param>
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IModelRelationship GetRelationship(Identity id, ISchemaRelationship metaclass, bool localOnly = true)
        {
            return base.GetRelationship(id, metaclass, localOnly) ?? _extendedMetaModel.GetRelationship(id, metaclass, localOnly);
        }       
    }
}