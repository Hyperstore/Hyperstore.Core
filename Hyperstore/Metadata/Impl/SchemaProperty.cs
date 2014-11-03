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
        private PropertyKind? _kind;
        private readonly ISchemaInfo _owner;
        #endregion Enums of MetaProperty (3)

        #region Properties of MetaProperty (3)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the owner.
        /// </summary>
        /// <value>
        ///  The owner.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaInfo Owner
        {
            get { return _owner; }
        }

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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the kind.
        /// </summary>
        /// <value>
        ///  The kind.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public PropertyKind Kind
        {
            get { 
                if( _kind.HasValue)
                return _kind.Value;
                    _kind = GetPropertyValue<PropertyKind>("Kind");
                return _kind.Value;
            }
            set { _kind = value; SetPropertyValue<PropertyKind>("Kind", value); }
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
        /// <param name="owner">
        ///  The owner.
        /// </param>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <param name="propertyMetaclass">
        ///  The property metaclass.
        /// </param>
        /// <param name="kind">
        ///  (Optional) the kind.
        /// </param>
        /// <param name="defaultValue">
        ///  (Optional)
        /// </param>
        /// <param name="implementedType">
        ///  (Optional) Type of the implemented.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaProperty(ISchemaInfo owner, string propertyName, ISchemaValueObject propertyMetaclass, PropertyKind kind = PropertyKind.Normal, object defaultValue = null, Type implementedType = null)
        {
            Contract.Requires(owner, "owner");
            Contract.RequiresNotEmpty(propertyName, "propertyName");

            _owner = owner;
            //if (InvalidPropertyNames.Any(p => String.Compare(p, propertyName, StringComparison.OrdinalIgnoreCase) == 0))
            //    throw new Exception(ExceptionMessages.InvalidPropertyNameCantBeAPropertyOfIModelElement);

            ConstructInternal(owner.Schema, implementedType ?? typeof(SchemaProperty), owner.Id.CreateMetaPropertyIdentity(propertyName), propertyName, null, PrimitivesSchema.SchemaPropertySchema,
                    (dm, melId, m) => new AddSchemaPropertyCommand(dm as ISchema, melId, (ISchemaEntity)m));

            // Attention ici toutes les propriétés doivent avoir été déclarées sous peine de rentrer dans une bouble infini
            // au niveau GetOrCreateProperty de ModelElement
            PropertySchema = propertyMetaclass;
            if (defaultValue != null)
                DefaultValue = defaultValue;
            Kind = kind;
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.ModelElement and optionally
        ///  releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
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
        protected override object Serialize(object data)
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