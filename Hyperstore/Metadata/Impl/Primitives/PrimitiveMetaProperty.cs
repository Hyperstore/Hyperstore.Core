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

#endregion

namespace Hyperstore.Modeling.Metadata.Primitives
{
    [DebuggerDisplay("Primitive Property {_name} Id={_id}")]
    internal class PrimitiveMetaProperty : PrimitiveMetaEntity, ISchemaProperty
    {
        private readonly ISchemaValueObject _metadata;

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
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public PrimitiveMetaProperty(PrimitivesSchema domainModel, Identity id, string name, ISchemaValueObject metadata)
            : base(domainModel, typeof(SchemaProperty), domainModel.SchemaEntitySchema, name: name, id: id)
        {
            DebugContract.Requires(domainModel, "domainModel");
            DebugContract.RequiresNotEmpty(name);
            DebugContract.Requires(metadata, "metadata");
            DebugContract.Requires(id, "id");

            _metadata = metadata;
            ((PrimitivesSchema) domainModel).RegisterMetadata(this);
        }

        public ISchemaInfo Owner { get { return null; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name of the property.
        /// </summary>
        /// <value>
        ///  The name of the property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string PropertyName
        {
            get { return Name; }
        }

        public PropertyKind Kind
        {
            get { return PropertyKind.Normal; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property schema.
        /// </summary>
        /// <value>
        ///  The property schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaValueObject PropertySchema
        {
            get { return _metadata; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override object Deserialize(SerializationContext ctx)
        {
            ctx.Schema = _metadata;
            return _metadata.Deserialize(ctx);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override object Serialize(object data)
        {
            return _metadata.Serialize(data);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "Property " + Name;
        }
    }
}