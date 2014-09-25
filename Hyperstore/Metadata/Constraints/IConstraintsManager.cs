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
namespace Hyperstore.Modeling.Metadata.Constraints
{
    internal interface IConstraintManagerInternal
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Check elements.
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        /// <returns>
        ///  A Hyperstore.Modeling.ISessionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Hyperstore.Modeling.ISessionResult CheckElements(System.Collections.Generic.IEnumerable<Hyperstore.Modeling.IModelElement> elements);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance has implicit constraints.
        /// </summary>
        /// <value>
        ///  true if this instance has implicit constraints, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool HasImplicitConstraints { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a constraint to 'constraint'.
        /// </summary>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <param name="constraint">
        ///  The constraint.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddConstraint(ISchemaProperty property, IConstraint constraint);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for constraints manager.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IConstraintsManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a constraint to 'constraint'.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="constraint">
        ///  The constraint.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddConstraint<T>(Hyperstore.Modeling.ISchemaElement schema, Hyperstore.Modeling.Metadata.Constraints.ICheckConstraint<T> constraint) where T : Hyperstore.Modeling.IModelElement;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates.
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        /// <param name="category">
        ///  (Optional) the category.
        /// </param>
        /// <returns>
        ///  A Hyperstore.Modeling.ISessionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Hyperstore.Modeling.ISessionResult Validate(System.Collections.Generic.IEnumerable<Hyperstore.Modeling.IModelElement> elements, string category = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="category">
        ///  (Optional) the category.
        /// </param>
        /// <returns>
        ///  A Hyperstore.Modeling.ISessionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Hyperstore.Modeling.ISessionResult Validate(IDomainModel domain, string category = null);

    }
}
