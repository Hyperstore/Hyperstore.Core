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
    public class SchemaEntity<T> : SchemaEntity where T : IModelEntity
    {
        #region Constructors of MetaClass (1)

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
        public SchemaEntity(ISchema schema, ISchemaEntity superEntity = null, string name = null)
            : base(schema, name, superEntity, implementedType: typeof(T))
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

            if (!ReflectionHelper.IsAssignableFrom(typeof(IModelElement), ImplementedType))
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