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
