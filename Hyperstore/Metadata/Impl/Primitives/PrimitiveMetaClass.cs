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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Hyperstore.Modeling.Utils;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling.Metadata.Primitives
{
    internal class PrimitiveMetaEntity<T> : PrimitiveMetaEntity
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public PrimitiveMetaEntity(PrimitivesSchema domainModel)
            : base(domainModel, typeof(T), domainModel.SchemaEntitySchema)
        {
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A primitive meta entity.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaEntity"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("Primitive {_name} Id={_id}")]
    public class PrimitiveMetaEntity : ISchemaEntity
    {
        private static int _sequence = 1;
        private readonly ISchema _domainModel;
        private readonly Identity _id;
        private readonly string _name;
        private readonly IDictionary<string, ISchemaProperty> _properties = new Dictionary<string, ISchemaProperty>();
        private readonly ISchemaElement _superClass;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected PrimitiveMetaEntity()
        {
            _sequence = Interlocked.Increment(ref _sequence);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="implementedType">
        ///  The type of the implemented.
        /// </param>
        /// <param name="superClass">
        ///  The super class.
        /// </param>
        /// <param name="name">
        ///  (Optional) The name.
        /// </param>
        /// <param name="id">
        ///  (Optional) The identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        internal PrimitiveMetaEntity(ISchema domainModel, Type implementedType, ISchemaElement superClass, string name = null, Identity id = null) : this()
        {
            DebugContract.Requires(domainModel, "domainModel");

            _domainModel = domainModel;
            ImplementedType = implementedType;
            _name = name ?? implementedType.FullName;
            _id = id ?? new Identity(domainModel.Name, _name);
            _superClass = superClass;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the properties.
        /// </summary>
        /// <value>
        ///  The properties.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<ISchemaProperty> Properties
        {
            get { return _properties.Values; }
        }

        IDomainModel IModelElement.DomainModel
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _domainModel; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the status.
        /// </summary>
        /// <value>
        ///  The status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ModelElementStatus Status
        {
            get { return ModelElementStatus.Created; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IHyperstore Store
        {
            [DebuggerStepThrough]
            get { return _domainModel.Store; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the type of the implemented.
        /// </summary>
        /// <value>
        ///  The type of the implemented.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Type ImplementedType { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the sequence.
        /// </summary>
        /// <value>
        ///  The sequence.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Sequence { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the super class.
        /// </summary>
        /// <value>
        ///  The super class.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement SuperClass
        {
            get { return _superClass; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id
        {
            [DebuggerStepThrough]
            get { return _id; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name.
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Name
        {
            [DebuggerStepThrough]
            get { return _name; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual object Serialize(object data)
        {
            if (data == null)
                return null;

            var mel = data as IModelElement;
            if (mel == null)
                throw new HyperstoreException(ExceptionMessages.InvalidClassSerialization);

            return mel.Id;
        }

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
        public virtual object Deserialize(SerializationContext ctx)
        {
            DebugContract.Requires(ctx);
            var upd = ctx.DomainModel as IUpdatableDomainModel;
            if (upd == null)
                throw new HyperstoreException(string.Format(ExceptionMessages.DomainModelIsReadOnlyCantCreateElementFormat, ctx.Id));

            var mel = upd.ModelElementFactory.InstanciateModelElement(this, ImplementedType ?? typeof (ModelEntity));
            if (mel is ISerializableModelElement)
                ((ISerializableModelElement) mel).OnDeserializing(this, ctx.DomainModel, ctx.Id.Key, ctx.StartId, ctx.EndId);

            return mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a property.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The property.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaProperty GetProperty(string name)
        {
            DebugContract.RequiresNotEmpty(name, "name");

            ISchemaProperty prop;
            if (_properties.TryGetValue(name, out prop))
                return prop;
            return _superClass != null ? _superClass.GetProperty(name) : null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Define property.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="metadata">
        ///  the metadata.
        /// </param>
        /// <param name="defaultValue">
        ///  (Optional) The default value.
        /// </param>
        /// <param name="kind">
        ///  (Optional) the kind.
        /// </param>
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaProperty DefineProperty(string name, ISchemaValueObject metadata, object defaultValue = null, PropertyKind kind = PropertyKind.Normal)
        {
            DebugContract.RequiresNotEmpty(name, "name");
            DebugContract.Requires(metadata, "metadata");

            var prop = new PrimitiveMetaProperty((PrimitivesSchema)Schema, new Identity(Schema.Name, Name + "." + name), name, metadata);
            _properties.Add(name, prop);
            return prop;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Define property.
        /// </summary>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaProperty DefineProperty(ISchemaProperty property)
        {
            DebugContract.Requires(property, "property");

            _properties.Add(property.Name, property);
            return property;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Define property.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="defaultValue">
        ///  (Optional) The default value.
        /// </param>
        /// <param name="kind">
        ///  (Optional) the kind.
        /// </param>
        /// <returns>
        ///  An ISchemaProperty.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaProperty DefineProperty<T>(string name, object defaultValue = null, PropertyKind kind = PropertyKind.Normal)
        {
            DebugContract.RequiresNotEmpty(name, "name");

            var metadata = Store.GetSchemaInfo<T>() as ISchemaValueObject;
            return DefineProperty(name, metadata, defaultValue);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the properties in this collection.
        /// </summary>
        /// <param name="recursive">
        ///  true to process recursively, false to process locally only.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the properties in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<ISchemaProperty> GetProperties(bool recursive)
        {
            foreach (var prop in _properties.Values)
            {
                yield return prop;
            }

            if (recursive && _superClass != null)
            {
                foreach (var prop in _superClass.GetProperties(true))
                {
                    yield return prop;
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets information describing the schema.
        /// </summary>
        /// <value>
        ///  Information describing the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement SchemaInfo
        {
            get { return _domainModel.Store.PrimitivesSchema.SchemaEntitySchema; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema.
        /// </summary>
        /// <value>
        ///  The schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchema Schema
        {
            [DebuggerStepThrough]
            get { return _domainModel as ISchema; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'metaClass' is a.
        /// </summary>
        /// <param name="metaClass">
        ///  The meta class.
        /// </param>
        /// <returns>
        ///  true if a, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsA(ISchemaInfo metaClass)
        {
            DebugContract.Requires(metaClass, "metaClass");

            // $/$ == $/0
            if (Id == _domainModel.Store.PrimitivesSchema.GeneratedSchemaEntitySchema.Id && metaClass.Id == _domainModel.Store.PrimitivesSchema.SchemaElementSchema.Id)
                return true;

            return (metaClass != null && metaClass.Id == Id) || (SuperClass != null && SuperClass.IsA(metaClass));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default value.
        /// </summary>
        /// <value>
        ///  The default value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual object DefaultValue
        {
            get
            {
                return ReflectionHelper.GetDefaultValue(ImplementedType);
                //if (ReflectionHelper.IsValueType(ImplementedType) && Nullable.GetUnderlyingType(ImplementedType) == null)
                //    return Activator.CreateInstance(ImplementedType);
                //return null;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is primitive.
        /// </summary>
        /// <value>
        ///  true if this instance is primitive, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsPrimitive
        {
            [DebuggerStepThrough]
            get { return true; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public PropertyValue GetPropertyValue(ISchemaProperty property)
        {
            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships in this collection.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="metadata">
        ///  (Optional) the metadata.
        /// </param>
        /// <param name="end">
        ///  (Optional) the end.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship metadata = null, IModelElement end = null)
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships in this collection.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="end">
        ///  (Optional) the end.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetRelationships<T>(IModelElement end = null) where T : IModelRelationship
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets property value.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetPropertyValue<T>(string propertyName)
        {
            throw new NotImplementedException();
        }

        void IModelElement.Remove()
        {
            throw new Hyperstore.Modeling.Commands.ReadOnlyException(ExceptionMessages.CantRemoveSchemaElementSchemaIsImmutable);
        }
    }
}