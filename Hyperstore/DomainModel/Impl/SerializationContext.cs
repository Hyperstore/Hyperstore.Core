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

using System.Diagnostics;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A serialization context.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class SerializationContext
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public SerializationContext(IDomainModel domainModel, Identity id, ISchemaInfo schemaElement)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(id, "id");
            Contract.Requires(schemaElement, "schemaElement");

            Id = id;
            DomainModel = domainModel;
            Schema = schemaElement;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="schemaProperty">
        ///  The schema property.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SerializationContext(ISchemaProperty schemaProperty, object value)
        {
            Contract.Requires(schemaProperty, "schemaProperty");
            Schema = schemaProperty.PropertySchema;
            Value = value;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="schemaValueObject">
        ///  The schema value object.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SerializationContext(ISchemaValueObject schemaValueObject, object value)
        {
            Contract.Requires(schemaValueObject, "schemaValueObject");
            Schema = schemaValueObject;
            Value = value;
        }

        [DebuggerStepThrough]
        internal SerializationContext(IDomainModel domainModel, ISchemaInfo schemaElement, IGraphNode v)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(v, "v");

            Id = v.Id;
            DomainModel = domainModel;
            StartId = v.StartId;
            EndId = v.EndId;
            Schema = schemaElement;
            EndSchemaId = v.EndSchemaId;
            StartSchemaId = v.StartSchemaId;
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
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Value { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the start.
        /// </summary>
        /// <value>
        ///  The identifier of the start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the end.
        /// </summary>
        /// <value>
        ///  The identifier of the end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the start schema.
        /// </summary>
        /// <value>
        ///  The identifier of the start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartSchemaId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the end schema.
        /// </summary>
        /// <value>
        ///  The identifier of the end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndSchemaId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the schema.
        /// </summary>
        /// <value>
        ///  The schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaInfo Schema { get; set; }
    }
}