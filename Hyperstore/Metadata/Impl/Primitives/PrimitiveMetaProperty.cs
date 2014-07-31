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
        public PrimitiveMetaProperty(ISchema domainModel, Identity id, string name, ISchemaValueObject metadata) : base(domainModel, typeof (SchemaProperty), PrimitivesSchema.SchemaEntitySchema, name: name, id: id)
        {
            DebugContract.Requires(domainModel, "domainModel");
            DebugContract.RequiresNotEmpty(name);
            DebugContract.Requires(metadata, "metadata");
            DebugContract.Requires(id, "id");

            _metadata = metadata;
            ((PrimitivesSchema) domainModel).RegisterMetadata(this);
        }

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
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string Serialize(object data, IJsonSerializer serializer)
        {
            return _metadata.Serialize(data, serializer);
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