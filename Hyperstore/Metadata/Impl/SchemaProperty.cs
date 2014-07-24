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

#region Imports (3)

using System.Linq;

#endregion Imports (3)

using System;
using System.Diagnostics;
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema property.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaInfo"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaProperty"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class SchemaProperty : SchemaInfo, ISchemaProperty
    {
        private string DebuggerDisplayString
        {
            get { return String.Format("Schema Property {0} Id={1}", Name, ((IModelElement)this).Id); }
        }

        //private readonly string[] InvalidPropertyNames = {"Id", "Status", "Start", "End", "DomainModel", "Metadata"};

        #region Enums of MetaProperty (3)

        private ISchemaValueObject _propertyMetadata;
        private ReferenceHandler _propertyMetadataReference;
        private object _defaultValue;
        private bool _defaultValueInitialized;
        private ISchemaProperty _defaultValueProperty;

        #endregion Enums of MetaProperty (3)

        #region Properties of MetaProperty (3)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default super class.
        /// </summary>
        /// <value>
        ///  The default super class.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected override ISchemaElement DefaultSuperClass
        {
            get { return PrimitivesSchema.SchemaPropertySchema; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property metadata.
        /// </summary>
        /// <value>
        ///  The property metadata.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaValueObject PropertySchema
        {
            get { return _propertyMetadata ?? (_propertyMetadata = _propertyMetadataReference.GetReference() as ISchemaValueObject); }
            private set { _propertyMetadataReference.SetReference(_propertyMetadata = value); }
        }

        #endregion Properties of MetaProperty (3)

        #region Constructors of MetaProperty (1)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaProperty()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="SchemaProperty" /> class.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <param name="propertyMetaclass">
        ///  The property metaclass.
        /// </param>
        /// <param name="defaultValue">
        ///  (Optional)
        /// </param>
        /// <param name="implementedType">
        ///  (Optional) Type of the implemented.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaProperty(ISchemaInfo owner, string propertyName, ISchemaValueObject propertyMetaclass, object defaultValue = null, Type implementedType = null)
        {
            Contract.Requires(owner, "owner");
            Contract.RequiresNotEmpty(propertyName, "propertyName");

            //if (InvalidPropertyNames.Any(p => String.Compare(p, propertyName, StringComparison.OrdinalIgnoreCase) == 0))
            //    throw new Exception(ExceptionMessages.InvalidPropertyNameCantBeAPropertyOfIModelElement);

            ConstructInternal(owner.Schema, implementedType ?? typeof(SchemaProperty), owner.Id.CreateMetaPropertyIdentity(propertyName), propertyName, null, PrimitivesSchema.SchemaPropertySchema,
                    (dm, melId, m) => new AddSchemaPropertyCommand(dm as ISchema, melId, (ISchemaEntity)m));

            // Attention ici toutes les propriétés doivent avoir été déclarées sous peine de rentrer dans une bouble infini
            // au niveau GetOrCreateProperty de ModelElement
            PropertySchema = propertyMetaclass;
            if (defaultValue != null)
                DefaultValue = defaultValue;
        }

        #endregion Constructors of MetaProperty (1)

        #region Methods of MetaProperty (2)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes the specified metadata.
        /// </summary>
        /// <param name="schemaElement">
        ///  The metadata.
        /// </param>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected override void Initialize(ISchemaElement schemaElement, IDomainModel domainModel)
        {
            base.Initialize(schemaElement, domainModel);
            _propertyMetadataReference = new ReferenceHandler(this, PrimitivesSchema.SchemaPropertyReferencesSchemaEntitySchema);
            _defaultValueProperty = ((IModelElement)this).SchemaInfo.GetProperty("DefaultValue");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _propertyMetadataReference.Dispose();
            _defaultValueProperty = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the default value.
        /// </summary>
        /// <value>
        ///  The default value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected override object DefaultValue
        {
            get
            {
                if (_defaultValueInitialized)
                    return _defaultValue;

                _defaultValueInitialized = true;
                var pv = GetPropertyValue(_defaultValueProperty);
                if (pv != null && pv.HasValue)
                    return _defaultValue = Deserialize(new SerializationContext(this, pv.Value));

                return _defaultValue = PropertySchema.DefaultValue;
            }
            set
            {
                _defaultValue = value;
                _defaultValueInitialized = true;
                SetPropertyValue("DefaultValue", Serialize(value));
            }
        }

        #endregion Methods of MetaProperty (2)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serializes the specified data.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override string Serialize(object data)
        {
            return PropertySchema.Serialize(data);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Deserializes an element from the specified context.
        /// </summary>
        /// <param name="ctx">
        ///  Serialization context.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override object Deserialize(SerializationContext ctx)
        {
            ctx.Schema = this.PropertySchema;
            return PropertySchema.Deserialize(ctx);
        }
    }
}