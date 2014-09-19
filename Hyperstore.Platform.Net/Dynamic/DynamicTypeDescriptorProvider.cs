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