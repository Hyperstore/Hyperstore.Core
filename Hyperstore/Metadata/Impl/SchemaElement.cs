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
using System.Linq;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Metadata.Primitives;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema element.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaInfo"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaElement"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("SchemaElement {DebuggerDisplay,nq}")]
    public abstract class SchemaElement : SchemaInfo, ISchemaElement
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaElement()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="SchemaEntity" /> class.
        /// </summary>
        /// <param name="schema">
        ///  The domain model.
        /// </param>
        /// <param name="implementedType">
        ///  Type of the implemented.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="id">
        ///  (Optional) The id.
        /// </param>
        /// <param name="superMetaClass">
        ///  (Optional) The super meta class.
        /// </param>
        /// <param name="metaclass">
        ///  (Optional) The metaclass.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaElement(ISchema schema, Type implementedType, string name, Identity id = null, ISchemaElement superMetaClass = null, ISchemaElement metaclass = null)
            : base(schema, implementedType, name, id, superMetaClass, metaclass)
        {
        }

        ISchemaProperty ISchemaElement.GetProperty(string name)
        {
            return GetProperty(name);
        }

        ISchemaProperty ISchemaElement.DefineProperty(string name, ISchemaValueObject property, object defaultValue, PropertyKind kind)
        {
            return DefineProperty(name, property, kind, defaultValue);
        }

        ISchemaProperty ISchemaElement.DefineProperty(ISchemaProperty property)
        {
            return DefineProperty(property);
        }

        ISchemaProperty ISchemaElement.DefineProperty<T>(string name, object defaultValue, PropertyKind kind)
        {
            return DefineProperty<T>(name, defaultValue, kind);
        }

        System.Collections.Generic.IEnumerable<ISchemaProperty> ISchemaElement.GetProperties(bool recursive)
        {
            return GetProperties(recursive);
        }

        private string DebuggerDisplay
        {
            get { return String.Format("Name={0}, Id={1}", Name, ((IModelElement) this).Id); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Deserializes an element from the specified context.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="ctx">
        ///  Serialization context.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override object Deserialize(SerializationContext ctx)
        {
            Contract.Requires(ctx, "ctx");
            var upd = ctx.DomainModel as IUpdatableDomainModel;
            if (upd == null)
                throw new ReadOnlyException(string.Format(ExceptionMessages.DomainModelIsReadOnlyCantCreateElementFormat, ctx.Id));

            var mel = upd.ModelElementFactory.InstanciateModelElement(ctx.Schema, ImplementedType ?? typeof(DynamicModelEntity));
            var element = mel as ISerializableModelElement;
            if (element != null)
            {
                var c = ctx.Schema as ISchemaElement;
                Debug.Assert(c != null);
                element.OnDeserializing(c, ctx.DomainModel, ctx.Id.Key, ctx.StartId, ctx.EndId, ctx.EndSchemaId);
            }

            return mel;
        }

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
            return null;
        }
    }
}