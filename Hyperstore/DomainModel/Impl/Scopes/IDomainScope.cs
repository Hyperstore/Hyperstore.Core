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
 
using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    internal interface IScope
    {
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for domain model extension.
    /// </summary>
    /// <seealso cref="T:IDomainModel"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IDomainScope : IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets updated properties.
        /// </summary>
        /// <returns>
        ///  The updated properties.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        System.Collections.Generic.IEnumerable<PropertyValue> GetUpdatedProperties();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get elements created in the scope
        /// </summary>
        /// <param name="schemaElement">
        ///  (Optional) the schema element.
        /// </param>
        /// <returns>
        ///  List of elements created (or updated) in the scope
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelElement> GetScopeElements(ISchemaElement schemaElement = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get deleted elements
        /// </summary>
        /// <returns>
        ///  List of elements deleted in the scope
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphNode> GetDeletedElements();
    }
}
