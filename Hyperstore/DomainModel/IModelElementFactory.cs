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

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for model element factory.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IModelElementFactory
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Instanciates the model element.
        /// </summary>
        /// <param name="elementSchema">
        ///  The element schema.
        /// </param>
        /// <param name="implementationType">
        ///  Type of the implementation.
        /// </param>
        /// <returns>
        ///  An IModelElement.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelElement InstanciateModelElement(ISchemaInfo elementSchema, Type implementationType);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the element.
        /// </summary>
        /// <param name="entitySchema">
        ///  The entity schema.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelEntity CreateEntity(ISchemaEntity entitySchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the relationship.
        /// </summary>
        /// <param name="relationshipSchema">
        ///  The relationship schema.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IModelRelationship CreateRelationship(ISchemaRelationship relationshipSchema, IModelElement start, IModelElement end);
    }
}