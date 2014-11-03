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
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema value object.
    /// </summary>
    /// <typeparam name="T">
    ///  .
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaValueObject"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class SchemaValueObject<T> : SchemaValueObject
    {
        private string DebuggerDisplayString
        {
            get { return String.Format("Schema ValueObject {0} Id={1}", Name, ((IModelElement)this).Id); }
        }

        #region Constructors of MetaValue (1)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaValueObject()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="name">
        ///  (Optional) The name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaValueObject(ISchema schema, string name = null)
            : base(schema, typeof(T), name, null)
        {
        }

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

            if (ReflectionHelper.IsGenericType(GetType(), typeof(SchemaValueObject<>)))
            {
                // Optimisation
                return Schema.Store.PrimitivesSchema.SchemaEntitySchema;
            }

            return base.EnsuresSchemaExists(domainModel, name);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema value object.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaInfo"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaValueObject"/>
    ///-------------------------------------------------------------------------------------------------
    public class SchemaValueObject : SchemaInfo, ISchemaValueObject
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaValueObject()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="SchemaValueObject{T}" /> class.
        /// </summary>
        /// <param name="schema">
        ///  The domain model.
        /// </param>
        /// <param name="implementedType">
        ///  The implemented type.
        /// </param>
        /// <param name="name">
        ///  (Optional) The name.
        /// </param>
        /// <param name="metaclass">
        ///  (Optional) the metaclass.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaValueObject(ISchema schema, Type implementedType, string name = null, ISchemaElement metaclass = null)
            : base(schema, implementedType, name, null, null, metaclass)
        {
            Contract.Requires(schema, "schema");
        }

        #endregion Constructors of MetaValue (1)
    }
}