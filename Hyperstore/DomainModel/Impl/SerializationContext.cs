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
        internal SerializationContext(IDomainModel domainModel, ISchemaInfo schemaElement, GraphNode v)
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