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
                return PrimitivesSchema.SchemaEntitySchema;
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

        #region Methods of MetaValue (1)

        #endregion Methods of MetaValue (1)

        #endregion Constructors of MetaValue (1)
    }
}