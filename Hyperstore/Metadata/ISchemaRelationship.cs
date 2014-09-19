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
 
namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for schema relationship.
    /// </summary>
    /// <seealso cref="T:IModelRelationship"/>
    /// <seealso cref="T:ISchemaElement"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ISchemaRelationship : ISchemaElement, IModelRelationship
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the cardinality.
        /// </summary>
        /// <value>
        ///  The cardinality.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Cardinality Cardinality { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is embedded.
        /// </summary>
        /// <value>
        ///  true if this instance is embedded, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool IsEmbedded { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start element schema
        /// </summary>
        /// <value>
        ///  The start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        new ISchemaElement Start { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end element schema
        /// </summary>
        /// <value>
        ///  The end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        new ISchemaElement End { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name of the start property.
        /// </summary>
        /// <value>
        ///  The name of the start property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string StartPropertyName { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name of the end property.
        /// </summary>
        /// <value>
        ///  The name of the end property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string EndPropertyName { get; }
    }
}