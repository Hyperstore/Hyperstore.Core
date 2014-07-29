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
 
namespace Hyperstore.Modeling.Validations
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for constraints manager.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IConstraintsManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance has constraints.
        /// </summary>
        /// <value>
        ///  true if this instance has constraints, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool HasConstraints { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the schema. </summary>
        /// <value> The schema. </value>
        ///-------------------------------------------------------------------------------------------------
        ISchema Schema { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register a constraint on a schema element.
        /// </summary>
        /// <typeparam name="T">
        ///  Type of the element to validate.
        /// </typeparam>
        /// <param name="schemaElement">
        ///  (Optional) Schema element of the element to validate.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  A constraint builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<T> On<T>(ISchemaElement schemaElement=null, string propertyName = null) where T : IModelElement;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register a constraint on a schema element.
        /// </summary>
        /// <param name="schemaElement">
        ///  (Optional) Schema element of the element to validate.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  A constraint builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<IModelElement> On(ISchemaElement schemaElement, string propertyName = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Register a constraint on a schema element.
        /// </summary>
        /// <param name="schemaElementName">
        ///  Schema element of the element to validate.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  A constraint builder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IConstraintBuilder<IModelElement> On(string schemaElementName, string propertyName = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates some elements using all the constraints of the specified category.
        /// </summary>
        /// <param name="categoryName">
        ///  Name of the category.
        /// </param>
        /// <param name="elements">
        ///  List of the elements to validate or null for all domain elements.
        /// </param>
        /// <returns>
        ///  The result of the validation.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IExecutionResult Validate(string categoryName, params IModelElement[] elements);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates some elements using all the constraintsof the domain.
        /// </summary>
        /// <param name="elements">
        ///  List of the elements to validate or null for all domain elements.
        /// </param>
        /// <returns>
        ///  The result of the validation.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IExecutionResult Validate(params IModelElement[] elements);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates some elements using all the constraints of the specified category.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="categoryName">
        ///  (Optional) Name of the category.
        /// </param>
        /// <returns>
        ///  The result of the validation.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IExecutionResult Validate(IDomainModel domain, string categoryName=null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Add a specific constraint.
        /// </summary>
        /// <param name="constraint">
        ///  The constraint.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddConstraint(IConstraint constraint);
    }
}