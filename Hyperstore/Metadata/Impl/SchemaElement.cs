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
    }
}