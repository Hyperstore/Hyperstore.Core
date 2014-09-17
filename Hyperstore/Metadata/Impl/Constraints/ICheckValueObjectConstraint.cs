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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for check value object constraint.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface ICheckValueObjectConstraint
    {
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for validation value object constraint.
    /// </summary>
    /// <seealso cref="T:ICheckValueObjectConstraint"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IValidationValueObjectConstraint : ICheckValueObjectConstraint
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the category.
        /// </summary>
        /// <value>
        ///  The category.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string Category { get; }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for check value object constraint.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:ICheckValueObjectConstraint"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ICheckValueObjectConstraint<T> : ICheckValueObjectConstraint
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the constraint operation.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ExecuteConstraint(T value, ConstraintContext ctx);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for validation value object constraint.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:ICheckValueObjectConstraint{T}"/>
    /// <seealso cref="T:IValidationValueObjectConstraint"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IValidationValueObjectConstraint<T> : ICheckValueObjectConstraint<T>, IValidationValueObjectConstraint
    {
    }
}
