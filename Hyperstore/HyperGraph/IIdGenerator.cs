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
 
namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for identifier generator.
    /// </summary>
    /// <seealso cref="T:IDomainService"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IIdGenerator : IDomainService
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current value.
        /// </summary>
        /// <value>
        ///  The current value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string CurrentValue { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Calculate a new value.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema element 
        /// </param>
        /// <returns>
        ///  A new identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Identity NextValue(ISchemaElement schemaElement);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  This method is called whenever a new element is added to the domain. 
        /// </summary>
        /// <param name="id">
        ///  An identifier
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Set(Identity id);
    }
}