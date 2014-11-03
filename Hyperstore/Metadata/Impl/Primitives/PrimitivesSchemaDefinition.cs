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
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Metadata.Primitives;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  The primitives schema definition.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.DomainConfiguration"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaDefinition"/>
    ///-------------------------------------------------------------------------------------------------
    public class PrimitivesSchemaDefinition : DomainConfiguration, ISchemaDefinition
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the name of the schema. </summary>
        /// <value> The name of the schema. </value>
        ///-------------------------------------------------------------------------------------------------
        string ISchemaDefinition.SchemaName
        {
            get { return "$"; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Defines the schema. </summary>
        /// <param name="schema">   The schema. </param>
        ///-------------------------------------------------------------------------------------------------
        void ISchemaDefinition.DefineSchema(ISchema schema)
        {
            DebugContract.Requires(schema);

            var metaModel = schema as PrimitivesSchema;

            // La méta classe de base de tous les métadonnées
            metaModel.RegisterMetadata(metaModel.SchemaElementSchema = new PrimitiveMetaEntity(schema, null, null, "SchemaElement", new Identity(schema.Name, "SEL")));
            metaModel.RegisterMetadata(metaModel.SchemaEntitySchema = new PrimitiveMetaEntity(schema, typeof(SchemaEntity), metaModel.SchemaElementSchema, null, new Identity(schema.Name, "SEN")));
            metaModel.RegisterMetadata(metaModel.GeneratedSchemaEntitySchema = new PrimitiveMetaEntity(schema, typeof(SchemaEntity), metaModel.SchemaElementSchema, typeof(GeneratedSchemaEntity).FullName, new Identity(schema.Name, "$EN")));
            metaModel.RegisterMetadata(metaModel.SchemaValueObjectSchema = new PrimitiveMetaEntity(schema, typeof(SchemaValueObject), metaModel.SchemaElementSchema, null, new Identity(schema.Name, "SVO")));

            // Les types primitifs simples
            metaModel.RegisterMetadata(metaModel.StringSchema = new StringPrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.BooleanSchema = new BooleanPrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.UInt16Schema = new UInt16Primitive(metaModel));
            metaModel.RegisterMetadata(metaModel.UInt32Schema = new UInt32Primitive(metaModel));
            metaModel.RegisterMetadata(metaModel.UInt64Schema = new UInt64Primitive(metaModel));
            metaModel.RegisterMetadata(metaModel.Int16Schema = new Int16Primitive(metaModel));
            metaModel.RegisterMetadata(metaModel.Int32Schema = new Int32Primitive(metaModel));
            metaModel.RegisterMetadata(metaModel.Int64Schema = new Int64Primitive(metaModel));
            metaModel.RegisterMetadata(metaModel.DateTimeSchema = new DateTimePrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.TimeSpanSchema = new TimeSpanPrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.DecimalSchema = new DecimalPrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.SingleSchema = new SinglePrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.DoubleSchema = new DoublePrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.GuidSchema = new GuidPrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.TypeSchema = new TypePrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.IdentitySchema = new IdentityPrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.SchemaPropertyReferencesSchemaEntitySchema = new PrimitiveMetaRelationship(metaModel, "SchemaPropertyReferencesSchemaElement", "PropertySchema", null, Cardinality.OneToOne));
            metaModel.RegisterMetadata(metaModel.CharSchema = new CharPrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.ByteSchema = new BytePrimitive(metaModel));
            metaModel.RegisterMetadata(metaModel.ByteArraySchema = new ByteArrayPrimitive(metaModel));

            // Les propriétés de la MetaClass (On a attendu que les types primtitifs soient enregistrés)
            metaModel.SchemaElementSchema.DefineProperty("ImplementedType", metaModel.TypeSchema);
            metaModel.SchemaElementSchema.DefineProperty("Name", metaModel.StringSchema);
            //metaModel.GeneratedSchemaEntitySchema.DefineProperty("ImplementedType", metaModel.TypeSchema);
            //metaModel.GeneratedSchemaEntitySchema.DefineProperty("Name", metaModel.StringSchema);

            var cardinality = new CardinalityPrimitive(metaModel);
            metaModel.RegisterMetadata(cardinality);

            // MetaProperty
            metaModel.RegisterMetadata(metaModel.SchemaPropertySchema = new PrimitiveMetaEntity<SchemaProperty>(metaModel));
            // PrimitivesDomainModel.MetaProperty.DefineProperty("PropertyMetadata", PrimitivesDomainModel.MetaPropertyReferencesMetadata);
            metaModel.SchemaPropertySchema.DefineProperty("DefaultValue", metaModel.StringSchema);
            metaModel.SchemaPropertySchema.DefineProperty("Kind", new EnumPrimitiveInternal(metaModel, typeof(PropertyKind)));

            // MetaRelationship
            metaModel.RegisterMetadata(metaModel.SchemaRelationshipSchema = new PrimitiveMetaRelationship(metaModel, new Identity(schema.Name, "SER")));
            metaModel.SchemaRelationshipSchema.DefineProperty("Cardinality", cardinality);
            metaModel.SchemaRelationshipSchema.DefineProperty("IsEmbedded", metaModel.BooleanSchema);
            metaModel.SchemaRelationshipSchema.DefineProperty("StartId", metaModel.IdentitySchema);
            metaModel.SchemaRelationshipSchema.DefineProperty("EndId", metaModel.IdentitySchema);
            metaModel.SchemaRelationshipSchema.DefineProperty("StartMetaclassId", metaModel.IdentitySchema);
            metaModel.SchemaRelationshipSchema.DefineProperty("EndMetaclassId", metaModel.IdentitySchema);
            metaModel.SchemaRelationshipSchema.DefineProperty("StartPropertyName", metaModel.StringSchema);
            metaModel.SchemaRelationshipSchema.DefineProperty("EndPropertyName", metaModel.StringSchema);

            // MetadataHasProperties : Relations vers les propriétés
            metaModel.RegisterMetadata(
                                       metaModel.SchemaElementHasPropertiesSchema =
                                               new PrimitiveMetaRelationship(metaModel, "SchemaElementHasPropertiesSchema", "Properties", null, Cardinality.OneToMany, true, null, metaModel.SchemaElementSchema, metaModel.SchemaPropertySchema));

            // ModelElementMetadata
            metaModel.RegisterMetadata(metaModel.ModelEntitySchema = new PrimitiveModelElementMetaClass(metaModel));

            // ModelRelationshipMetadata
            metaModel.RegisterMetadata(metaModel.ModelRelationshipSchema = new PrimitiveModelRelationshipMetaClass(metaModel));

            // SuperTypeRelationship : Relation vers le superType
            metaModel.RegisterMetadata(metaModel.SchemaElementReferencesSuperElementSchema = new PrimitiveMetaRelationship(metaModel, "SchemaElementReferencesSuperElement", "Super", "null", Cardinality.OneToOne));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [schema loaded].
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ISchemaDefinition.OnSchemaLoaded(ISchema domainModel)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Creates a schema. </summary>
        /// <param name="container">   The domain services. </param>
        /// <returns>   The new schema. </returns>
        ///-------------------------------------------------------------------------------------------------
        ISchema<T> ISchemaDefinition.CreateSchema<T>(IServicesContainer container)
        {
            return new PrimitivesSchema(container) as ISchema<T>;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the behavior.
        /// </summary>
        /// <value>
        ///  The behavior.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        DomainBehavior ISchemaDefinition.Behavior { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets dependent schemas.
        /// </summary>
        /// <returns>
        ///  The dependent schemas.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Collections.Generic.IEnumerable<ISchemaDefinition> ISchemaDefinition.GetDependentSchemas()
        {
            yield break;
        }
    }
}