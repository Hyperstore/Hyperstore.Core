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