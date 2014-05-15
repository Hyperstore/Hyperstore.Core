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
using System.ComponentModel;

#endregion

namespace Hyperstore.Modeling.Dynamic
{
    internal class DynamicTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly DynamicModelEntity _element;

        internal DynamicTypeDescriptor(DynamicModelEntity element)
        {
            DebugContract.Requires(element);
            _element = element;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return new TypeConverter();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return EventDescriptorCollection.Empty;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return EventDescriptorCollection.Empty;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return ((ICustomTypeDescriptor) this).GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            var properties = new List<PropertyDescriptor>();
            foreach (var prop in ((IModelElement) _element).SchemaInfo.GetProperties(true))
            {
                properties.Add(new DynamicPropertyDescriptor(_element, prop));
            }

            return new PropertyDescriptorCollection(properties.ToArray());
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return _element;
        }
    }
}