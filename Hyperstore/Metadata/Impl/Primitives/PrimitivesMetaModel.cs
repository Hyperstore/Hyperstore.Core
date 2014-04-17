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
using System.Collections.Generic;
using System.Linq;
using Hyperstore.Modeling.Validations;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Commands;
using System.Diagnostics;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  The primitives schema.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISchema"/>
    ///-------------------------------------------------------------------------------------------------
    public class PrimitivesSchema : InternalSchema
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Name of the domain model.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string DomainModelName = "$"; // Name of the domain model

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        internal PrimitivesSchema(IDependencyResolver resolver)
            : base(resolver, "", DomainModelName)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override ISchemaInfo GetSchemaInfo(Identity id, bool throwErrorIfNotExists)
        {
            if (id == Identity.Empty)
                return SchemaEntitySchema;

            return base.GetSchemaInfo(id, throwErrorIfNotExists);
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the string schema.
        /// </summary>
        /// <value>
        ///  The string schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject StringSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the boolean schema.
        /// </summary>
        /// <value>
        ///  The boolean schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject BooleanSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the date time schema.
        /// </summary>
        /// <value>
        ///  The date time schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject DateTimeSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the time span schema.
        /// </summary>
        /// <value>
        ///  The time span schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject TimeSpanSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the single schema.
        /// </summary>
        /// <value>
        ///  The single schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject SingleSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the decimal schema.
        /// </summary>
        /// <value>
        ///  The decimal schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject DecimalSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the int 16 schema.
        /// </summary>
        /// <value>
        ///  The u int 16 schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject UInt16Schema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the int 32 schema.
        /// </summary>
        /// <value>
        ///  The u int 32 schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject UInt32Schema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the int 64 schema.
        /// </summary>
        /// <value>
        ///  The u int 64 schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject UInt64Schema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the int 16 schema.
        /// </summary>
        /// <value>
        ///  The int 16 schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject Int16Schema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the int 64 schema.
        /// </summary>
        /// <value>
        ///  The int 64 schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject Int64Schema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the int 32 schema.
        /// </summary>
        /// <value>
        ///  The int 32 schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject Int32Schema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the double schema.
        /// </summary>
        /// <value>
        ///  The double schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject DoubleSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the unique identifier schema.
        /// </summary>
        /// <value>
        ///  The unique identifier schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject GuidSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the character schema.
        /// </summary>
        /// <value>
        ///  The character schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject CharSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the byte schema.
        /// </summary>
        /// <value>
        ///  The byte schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject ByteSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the byte array schema.
        /// </summary>
        /// <value>
        ///  The byte array schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject ByteArraySchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity schema.
        /// </summary>
        /// <value>
        ///  The schema entity schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaEntity SchemaEntitySchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema element schema.
        /// </summary>
        /// <value>
        ///  The schema element schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaEntity SchemaElementSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema value object schema.
        /// </summary>
        /// <value>
        ///  The schema value object schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaEntity SchemaValueObjectSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the generated schema entity schema.
        /// </summary>
        /// <value>
        ///  The generated schema entity schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaEntity GeneratedSchemaEntitySchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema property schema.
        /// </summary>
        /// <value>
        ///  The schema property schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaEntity SchemaPropertySchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identity schema.
        /// </summary>
        /// <value>
        ///  The identity schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject IdentitySchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the type schema.
        /// </summary>
        /// <value>
        ///  The type schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaValueObject TypeSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship schema.
        /// </summary>
        /// <value>
        ///  The schema relationship schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaRelationship SchemaRelationshipSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the model entity schema.
        /// </summary>
        /// <value>
        ///  The model entity schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaEntity ModelEntitySchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the model relationship schema.
        /// </summary>
        /// <value>
        ///  The model relationship schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaRelationship ModelRelationshipSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the schema element has properties schema.
        /// </summary>
        /// <value>
        ///  The schema element has properties schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaRelationship SchemaElementHasPropertiesSchema { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema element references super element schema.
        /// </summary>
        /// <value>
        ///  The schema element references super element schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static ISchemaRelationship SchemaElementReferencesSuperElementSchema { get; internal set; }
        internal static ISchemaRelationship SchemaPropertyReferencesSchemaEntitySchema { get; set; }

    }
}