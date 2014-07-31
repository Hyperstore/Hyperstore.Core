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
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Metadata.Primitives;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    internal class PrimitivesSchemaDefinition : ISchemaDefinition
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the name of the schema. </summary>
        /// <value> The name of the schema. </value>
        ///-------------------------------------------------------------------------------------------------
        public string SchemaName
        {
            get { return "$"; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Defines the schema. </summary>
        /// <param name="schema">   The schema. </param>
        ///-------------------------------------------------------------------------------------------------
        public void DefineSchema(ISchema schema)
        {
            DebugContract.Requires(schema);

            var metaModel = schema as PrimitivesSchema;

            // La méta classe de base de tous les métadonnées
            metaModel.RegisterMetadata(PrimitivesSchema.SchemaElementSchema = new PrimitiveMetaEntity(schema, null, null, "SchemaElement", new Identity(schema.Name, "SEL")));
            metaModel.RegisterMetadata(PrimitivesSchema.SchemaEntitySchema = new PrimitiveMetaEntity(schema, typeof(SchemaEntity), PrimitivesSchema.SchemaElementSchema, null, new Identity(schema.Name, "SEN")));
            metaModel.RegisterMetadata(PrimitivesSchema.GeneratedSchemaEntitySchema = new PrimitiveMetaEntity(schema, typeof(SchemaEntity), PrimitivesSchema.SchemaElementSchema, typeof(GeneratedSchemaEntity).FullName, new Identity(schema.Name, "$EN")));
            metaModel.RegisterMetadata(PrimitivesSchema.SchemaValueObjectSchema = new PrimitiveMetaEntity(schema, typeof(SchemaValueObject), PrimitivesSchema.SchemaElementSchema, null, new Identity(schema.Name, "SVO")));

            // Les types primitifs simples
            metaModel.RegisterMetadata(PrimitivesSchema.StringSchema = new StringPrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.BooleanSchema = new BooleanPrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.UInt16Schema = new UInt16Primitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.UInt32Schema = new UInt32Primitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.UInt64Schema = new UInt64Primitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.Int16Schema = new Int16Primitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.Int32Schema = new Int32Primitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.Int64Schema = new Int64Primitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.DateTimeSchema = new DateTimePrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.TimeSpanSchema = new TimeSpanPrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.DecimalSchema = new DecimalPrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.SingleSchema = new SinglePrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.DoubleSchema = new DoublePrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.GuidSchema = new GuidPrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.TypeSchema = new TypePrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.IdentitySchema = new IdentityPrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.SchemaPropertyReferencesSchemaEntitySchema = new PrimitiveMetaRelationship(schema, "SchemaPropertyReferencesSchemaElement", "PropertySchema", null, Cardinality.OneToOne));
            metaModel.RegisterMetadata(PrimitivesSchema.CharSchema = new CharPrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.ByteSchema = new BytePrimitive(schema));
            metaModel.RegisterMetadata(PrimitivesSchema.ByteArraySchema = new ByteArrayPrimitive(schema));

            // Les propriétés de la MetaClass (On a attendu que les types primtitifs soient enregistrés)
            PrimitivesSchema.SchemaElementSchema.DefineProperty("ImplementedType", PrimitivesSchema.TypeSchema);
            PrimitivesSchema.SchemaElementSchema.DefineProperty("Name", PrimitivesSchema.StringSchema);
            //PrimitivesSchema.GeneratedSchemaEntitySchema.DefineProperty("ImplementedType", PrimitivesSchema.TypeSchema);
            //PrimitivesSchema.GeneratedSchemaEntitySchema.DefineProperty("Name", PrimitivesSchema.StringSchema);

            var cardinality = new CardinalityPrimitive(schema);
            metaModel.RegisterMetadata(cardinality); 

            // MetaProperty
            metaModel.RegisterMetadata(PrimitivesSchema.SchemaPropertySchema = new PrimitiveMetaEntity<SchemaProperty>(schema));
            // PrimitivesDomainModel.MetaProperty.DefineProperty("PropertyMetadata", PrimitivesDomainModel.MetaPropertyReferencesMetadata);
            PrimitivesSchema.SchemaPropertySchema.DefineProperty("DefaultValue", PrimitivesSchema.StringSchema);

            // MetaRelationship
            metaModel.RegisterMetadata(PrimitivesSchema.SchemaRelationshipSchema = new PrimitiveMetaRelationship(schema, new Identity(schema.Name, "SER")));
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("Cardinality", cardinality);
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("IsEmbedded", PrimitivesSchema.BooleanSchema);
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("StartId", PrimitivesSchema.IdentitySchema);
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("EndId", PrimitivesSchema.IdentitySchema);
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("StartMetaclassId", PrimitivesSchema.IdentitySchema);
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("EndMetaclassId", PrimitivesSchema.IdentitySchema);
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("StartPropertyName", PrimitivesSchema.StringSchema);
            PrimitivesSchema.SchemaRelationshipSchema.DefineProperty("EndPropertyName", PrimitivesSchema.StringSchema);

            // MetadataHasProperties : Relations vers les propriétés
            metaModel.RegisterMetadata(
                                       PrimitivesSchema.SchemaElementHasPropertiesSchema =
                                               new PrimitiveMetaRelationship(schema, "SchemaElementHasPropertiesSchema", "Properties", null, Cardinality.OneToMany, true, null, PrimitivesSchema.SchemaElementSchema, PrimitivesSchema.SchemaPropertySchema));

            // ModelElementMetadata
            metaModel.RegisterMetadata(PrimitivesSchema.ModelEntitySchema = new PrimitiveModelElementMetaClass(schema));

            // ModelRelationshipMetadata
            metaModel.RegisterMetadata(PrimitivesSchema.ModelRelationshipSchema = new PrimitiveModelRelationshipMetaClass(schema));

            // SuperTypeRelationship : Relation vers le superType
            metaModel.RegisterMetadata(PrimitivesSchema.SchemaElementReferencesSuperElementSchema = new PrimitiveMetaRelationship(schema, "SchemaElementReferencesSuperElement", "Super", "null", Cardinality.OneToOne));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Useses the specified factory. </summary>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <typeparam name="TService"> Type of the service. </typeparam>
        /// <param name="factory">  The factory. </param>
        /// <returns>   An ISchemaDefinition. </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaDefinition Uses<TService>(Func<IDependencyResolver, TService> factory) where TService : class
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Prepare dependency resolver. </summary>
        /// <param name="defaultDependencyResolver">    The default dependency resolver. </param>
        /// <returns>   An IDependencyResolver. </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDependencyResolver PrepareDependencyResolver(IDependencyResolver defaultDependencyResolver)
        {
            return defaultDependencyResolver;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Called when [schema loaded]. </summary>
        /// <param name="domainModel">  The domain model. </param>
        /// ### <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is unimplemented.
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        public void OnSchemaLoaded(ISchema domainModel)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Registers the in event bus. </summary>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="outputProperty">   The output property. </param>
        /// <param name="inputProperty">    (Optional) the input property. </param>
        /// <returns>   An ISchemaDefinition. </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaDefinition RegisterInEventBus( Messaging.ChannelPolicy outputProperty, Messaging.ChannelPolicy inputProperty = null)
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Uses identifier generator. </summary>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="factory">  The factory. </param>
        /// <returns>   An ISchemaDefinition. </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaDefinition UsesIdGenerator(Func<IDependencyResolver, IIdGenerator> factory)
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Uses graph adapter. </summary>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="factory">  The factory. </param>
        /// <returns>   An ISchemaDefinition. </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaDefinition UsesGraphAdapter(Func<IDependencyResolver, IGraphAdapter> factory)
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Creates a schema. </summary>
        /// <param name="domainResolver">   The domain resolver. </param>
        /// <returns>   The new schema. </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchema CreateSchema(IDependencyResolver domainResolver)
        {
            return new PrimitivesSchema(domainResolver);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Loads dependent schemas. </summary>
        /// <param name="store">    The store. </param>
        ///-------------------------------------------------------------------------------------------------
        public void LoadDependentSchemas(IHyperstore store)
        {
        }
    }
}