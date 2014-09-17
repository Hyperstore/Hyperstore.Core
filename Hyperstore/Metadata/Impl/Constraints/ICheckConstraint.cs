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
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    internal interface ICheckConstraint
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the constraint operation.
        /// </summary>
        /// <param name="mel">
        ///  The mel.
        /// </param>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ExecuteConstraint(IModelElement mel, ConstraintContext ctx);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for check constraint.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    public interface ICheckConstraint<T> where T : IModelElement
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the constraint operation.
        /// </summary>
        /// <param name="mel">
        ///  The mel.
        /// </param>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ExecuteConstraint(T mel, ConstraintContext ctx);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for validation constraint.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:ICheckConstraint{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IValidationConstraint<T> : ICheckConstraint<T> where T : IModelElement
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
}
