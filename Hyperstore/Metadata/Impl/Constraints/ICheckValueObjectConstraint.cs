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
    public interface IConstraint
    {
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for validation value object constraint.
    /// </summary>
    /// <seealso cref="T:ICheckValueObjectConstraint"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IValidationValueObjectConstraint : IConstraint
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
    public interface ICheckValueObjectConstraint<T> : IConstraint
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the constraint operation.
        /// </summary>
        /// <param name="self">
        ///  The value to test.
        /// </param>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ExecuteConstraint(T self, ConstraintContext ctx);
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
