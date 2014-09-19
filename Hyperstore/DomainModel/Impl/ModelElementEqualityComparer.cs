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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A model element comparer.
    /// </summary>
    /// <seealso cref="T:System.Collections.Generic.IEqualityComparer{Hyperstore.Modeling.IModelElement}"/>
    /// <seealso cref="T:System.Collections.IEqualityComparer"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelElementComparer : IEqualityComparer<IModelElement>, IEqualityComparer
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Tests if two IModelElement objects are considered equal.
        /// </summary>
        /// <param name="x">
        ///  I model element to be compared.
        /// </param>
        /// <param name="y">
        ///  I model element to be compared.
        /// </param>
        /// <returns>
        ///  true if the objects are considered equal, false if they are not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool Equals(IModelElement x, IModelElement y)
        {
            return x != null && y != null && x.Id == y.Id;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Calculates a hash code for this instance.
        /// </summary>
        /// <param name="obj">
        ///  The object.
        /// </param>
        /// <returns>
        ///  A hash code for this instance.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int GetHashCode(IModelElement obj)
        {
            return obj.Id.GetHashCode();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">
        ///  I model element to be compared.
        /// </param>
        /// <param name="y">
        ///  I model element to be compared.
        /// </param>
        /// <returns>
        ///  true if the specified objects are equal; otherwise, false.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool Equals(object x, object y)
        {
            return Equals(x as IModelElement, y as IModelElement);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">
        ///  The object.
        /// </param>
        /// <returns>
        ///  A hash code for the specified object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}
