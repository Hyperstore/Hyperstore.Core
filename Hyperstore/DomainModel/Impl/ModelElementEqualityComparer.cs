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
