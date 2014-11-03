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

#endregion

namespace Hyperstore.Modeling.Metadata.Primitives
{
    internal class PrimitiveMetaRelationship : PrimitiveMetaEntity, ISchemaRelationship
    {
        private readonly ISchemaElement _end;
        private readonly ISchemaElement _start;
        private readonly string _startPropertyName;
        private readonly string _endPropertyName;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="id">
        ///  the identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public PrimitiveMetaRelationship(PrimitivesSchema domainModel, Identity id)
            : base(domainModel, typeof(SchemaRelationship), domainModel.SchemaEntitySchema, null, id)
        {
            DebugContract.Requires(domainModel);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="startPropertyName">
        ///  The start property name.
        /// </param>
        /// <param name="endPropertyName">
        ///  The end property name.
        /// </param>
        /// <param name="cardinality">
        ///  The cardinality.
        /// </param>
        /// <param name="isEmbedded">
        ///  (Optional) true if this instance is embedded, false if not.
        /// </param>
        /// <param name="id">
        ///  (Optional) the identifier.
        /// </param>
        /// <param name="start">
        ///  (Optional) the start.
        /// </param>
        /// <param name="end">
        ///  (Optional) the end.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public PrimitiveMetaRelationship(ISchema domainModel, string name, string startPropertyName, string endPropertyName, Cardinality cardinality, bool isEmbedded = false, Identity id = null, ISchemaElement start = null, ISchemaElement end = null)
            : base(domainModel, typeof(ModelRelationship), null, name, id: id)
        {
            DebugContract.Requires(domainModel, "domainModel");
            Contract.RequiresNotEmpty(name, "name");

            Cardinality = cardinality;
            IsEmbedded = isEmbedded;
            this._start = start;
            this._end = end;
            this._startPropertyName = startPropertyName;
            this._endPropertyName = endPropertyName;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is embedded.
        /// </summary>
        /// <value>
        ///  true if this instance is embedded, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsEmbedded { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the cardinality.
        /// </summary>
        /// <value>
        ///  The cardinality.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Cardinality Cardinality { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override object Deserialize(SerializationContext ctx)
        {
            DebugContract.Requires(ctx);

            var upd = ctx.DomainModel as IUpdatableDomainModel;
            if (upd == null)
                throw new Hyperstore.Modeling.Commands.ReadOnlyException(string.Format( ExceptionMessages.DomainModelIsReadOnlyCantCreateElementFormat, ctx.Id));

            var mel = upd.ModelElementFactory.InstanciateModelElement(this, ImplementedType ?? typeof (ModelRelationship));
            if (mel is ISerializableModelElement)
                ((ISerializableModelElement) mel).OnDeserializing(this, ctx.DomainModel, ctx.Id.Key, ctx.StartId, ctx.EndId, ctx.EndSchemaId);

            return mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship.
        /// </summary>
        /// <value>
        ///  The schema relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaRelationship SchemaRelationship
        {
            get { return ((IModelRelationship) this).SchemaRelationship; }
        }

        #region IModelRelationship Members

        ISchemaElement ISchemaRelationship.Start
        {
            get { return this._start ?? this.Schema.Store.PrimitivesSchema.SchemaElementSchema; }
        }

        ISchemaElement ISchemaRelationship.End
        {
            get { return this._end ?? Schema.Store.PrimitivesSchema.SchemaElementSchema; }
        }

        IModelElement IModelRelationship.Start
        {
            get { return ((ISchemaRelationship)this).Start; }
        }

        IModelElement IModelRelationship.End
        {
            get { return ((ISchemaRelationship)this).End;  }
        }

        Identity IModelRelationship.EndId
        {
            get { return ((ISchemaRelationship)this).End.Id; }
        }

        Identity IModelRelationship.EndSchemaId
        {
            get { return ((ISchemaRelationship)this).End.SchemaInfo.Id; }
        }
        #endregion

        string ISchemaRelationship.StartPropertyName
        {
            get { return _startPropertyName; }
        }

        string ISchemaRelationship.EndPropertyName
        {
            get { return _endPropertyName; }
        }
    }
}