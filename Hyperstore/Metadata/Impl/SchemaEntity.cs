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
using System.Diagnostics;
using System.Linq;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema entity.
    /// </summary>
    /// <typeparam name="T">
    ///  .
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaEntity"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("SchemaEntity {Name, nq}")]
    public class SchemaEntity<T> : SchemaEntity where T:IModelEntity
    {
        #region Constructors of MetaClass (1)

#if NETFX_CORE
        protected SchemaEntity()
        {
        }
#endif

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="SchemaEntity{T}" /> class.
        /// </summary>
        /// <param name="schema">
        ///  The domain model.
        /// </param>
        /// <param name="superEntity">
        ///  (Optional) The super meta class.
        /// </param>
        /// <param name="name">
        ///  (Optional) The name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaEntity(ISchema schema, ISchemaEntity superEntity = null, string name = null) : base(schema, name, superEntity, implementedType: typeof (T))
        {
            Contract.Requires(schema, "schema");
        }

        #endregion Constructors of MetaClass (1)

        #region Methods of MetaClass (1)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ensureses the metadata exists.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  An ISchemaElement.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override ISchemaElement EnsuresSchemaExists(IDomainModel domainModel, string name)
        {
            DebugContract.Requires(domainModel);
            DebugContract.RequiresNotEmpty(name);

            //if (ReflectionHelper.IsGenericType(GetType(), typeof (SchemaEntity<>)))
            //{
            //    // Optimisation
            //    return PrimitivesSchema.SchemaEntitySchema;
            //}

            return base.EnsuresSchemaExists(domainModel, name);
        }

        #endregion Methods of MetaClass (1)
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema entity.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaElement"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaEntity"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("SchemaEntity {Name, nq}")]
    public class SchemaEntity : SchemaElement, ISchemaEntity
    {
        #region Constructors of MetaClass (2)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaEntity()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="SchemaEntity" /> class.
        /// </summary>
        /// <param name="schema">
        ///  Owner meta model.
        /// </param>
        /// <param name="name">
        ///  Unique name of the MetaClass.
        /// </param>
        /// <param name="superEntity">
        ///  (Optional) The super meta class.
        /// </param>
        /// <param name="metaclass">
        ///  (Optional)
        /// </param>
        /// <param name="implementedType">
        ///  (Optional) Type of the implemented. Default value is DynamicModelElement.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaEntity(ISchema schema, string name, ISchemaEntity superEntity = null, ISchemaEntity metaclass = null, Type implementedType = null) 
            : this(schema, implementedType, name, null, superEntity, metaclass)
        {
            Contract.Requires(schema, "schema");
            Contract.Requires(implementedType != null || name != null, "name");

            if (!ReflectionHelper.IsAssignableFrom( typeof(IModelElement), ImplementedType))
                throw new Exception("SchemaEntity must describes a type implementing IModelEntity");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="SchemaEntity" /> class.
        /// </summary>
        /// <param name="schema">
        ///  The domain model.
        /// </param>
        /// <param name="implementedType">
        ///  Type of the implemented.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="id">
        ///  (Optional) The id.
        /// </param>
        /// <param name="superEntity">
        ///  (Optional) The super meta class.
        /// </param>
        /// <param name="metaclass">
        ///  (Optional) The metaclass.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaEntity(ISchema schema, Type implementedType, string name, Identity id = null, ISchemaEntity superEntity = null, ISchemaEntity metaclass = null) 
            : base(schema, implementedType, name, id, superEntity, metaclass)
        {
        }

        #endregion Constructors of MetaClass (2)

        //ISchemaEntity IModelEntity.SchemaEntity
        //{
        //    get { return ((IModelElement)this).SchemaInfo as ISchemaEntity; }
        //}

        //IDomainModel IModelEntity.DomainModel
        //{
        //    get { return Container as IDomainModel; }
        //}
    }
}