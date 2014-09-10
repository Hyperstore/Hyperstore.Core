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

#endregion

namespace Hyperstore.Modeling.Validations
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for constraint builder.
    /// </summary>
    /// <typeparam name="T">
    ///  .
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    public interface IConstraintBuilder<out T> 
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Checks the specified expression.
        /// </summary>
        /// <param name="expression">
        ///  The expression.
        /// </param>
        /// <param name="message">
        ///  (Optional) The message.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<T> Check(Func<T, bool> expression, string message = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Checks the specified expression.
        /// </summary>
        /// <param name="expression">
        ///  The expression.
        /// </param>
        /// <param name="message">
        ///  The message.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<T> Check(Func<T, bool> expression, DiagnosticMessage message);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Checks the specified custom.
        /// </summary>
        /// <param name="custom">
        ///  The custom.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<T> Check(IConstraint<T> custom);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Implicits this instance.
        /// </summary>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<T> Implicit();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Converts this instance to a warning.
        /// </summary>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<T> AsWarning();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Categories the constraint.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<T> Category(string name);
    }
}