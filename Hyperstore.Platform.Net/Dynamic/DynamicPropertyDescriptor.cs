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
    // TODO 
    // ajouter 
    //    [TypeDescriptionProvider(typeof (DynamicTypeDescriptorProvider))]
    // sur DynamicModelEntity
    internal class DynamicPropertyDescriptor : PropertyDescriptor
    {
        private readonly DynamicModelEntity _element;
        private readonly ISchemaProperty _property;

        internal DynamicPropertyDescriptor(DynamicModelEntity owner, ISchemaProperty property)
            : base(property.Name, null)
        {
            DebugContract.Requires(owner);
            DebugContract.Requires(property);

            _element = owner;
            _property = property;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, gets the type of the component this property is bound to.
        /// </summary>
        /// <value>
        ///  A <see cref="T:System.Type" /> that represents the type of component this property is bound
        ///  to. When the
        ///  <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)" /> or
        ///  <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)" />
        ///  methods are invoked, the object specified might be an instance of this type.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public override Type ComponentType
        {
            get { return typeof (DynamicModelEntity); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, gets a value indicating whether this property is read-
        ///  only.
        /// </summary>
        /// <value>
        ///  true if the property is read-only; otherwise, false.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public override bool IsReadOnly
        {
            get { return false; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, gets the type of the property.
        /// </summary>
        /// <value>
        ///  A <see cref="T:System.Type" /> that represents the type of the property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public override Type PropertyType
        {
            get { return _property.PropertySchema.ImplementedType; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, returns whether resetting an object changes its value.
        /// </summary>
        /// <exception cref="NotImplementedException">
        ///  Thrown when the requested operation is unimplemented.
        /// </exception>
        /// <param name="component">
        ///  The component to test for reset capability.
        /// </param>
        /// <returns>
        ///  true if resetting the component changes its value; otherwise, false.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, gets the current value of the property on a component.
        /// </summary>
        /// <param name="component">
        ///  The component with the property for which to retrieve the value.
        /// </param>
        /// <returns>
        ///  The value of a property for a given component.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override object GetValue(object component)
        {
            return _element.TryGetProperty(_property.Name);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, resets the value for this property of the component to
        ///  the default value.
        /// </summary>
        /// <param name="component">
        ///  The component with the property value that is to be reset to the default value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public override void ResetValue(object component)
        {
            _element.TrySetProperty(_property.Name, _property.DefaultValue);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, sets the value of the component to a different value.
        /// </summary>
        /// <param name="component">
        ///  The component with the property value that is to be set.
        /// </param>
        /// <param name="value">
        ///  The new value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public override void SetValue(object component, object value)
        {
            _element.TrySetProperty(_property.Name, value);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  When overridden in a derived class, determines a value indicating whether the value of this
        ///  property needs to be persisted.
        /// </summary>
        /// <param name="component">
        ///  The component with the property to be examined for persistence.
        /// </param>
        /// <returns>
        ///  true if the property should be persisted; otherwise, false.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override bool ShouldSerializeValue(object component)
        {
            return !Equals(_element.TryGetProperty(_property.Name), _property.DefaultValue);
        }
    }
}