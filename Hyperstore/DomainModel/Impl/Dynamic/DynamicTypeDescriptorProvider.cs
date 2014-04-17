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
using System.ComponentModel;

#endregion

namespace Hyperstore.Modeling.Dynamic
{
    internal class DynamicTypeDescriptorProvider : TypeDescriptionProvider
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a custom type descriptor for the given type and object.
        /// </summary>
        /// <param name="objectType">
        ///  The type of object for which to retrieve the type descriptor.
        /// </param>
        /// <param name="instance">
        ///  An instance of the type. Can be null if no instance was passed to the
        ///  <see cref="T:System.ComponentModel.TypeDescriptor" />.
        /// </param>
        /// <returns>
        ///  An <see cref="T:System.ComponentModel.ICustomTypeDescriptor" /> that can provide metadata for
        ///  the type.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new DynamicTypeDescriptor((DynamicModelEntity) instance);
        }
    }
}