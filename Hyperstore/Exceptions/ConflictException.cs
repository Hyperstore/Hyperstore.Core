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

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Exception for signalling conflict errors.
    /// </summary>
    /// <seealso cref="T:System.Exception"/>
    ///-------------------------------------------------------------------------------------------------
    public class ConflictException : HyperstoreException
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier that owns this item.
        /// </summary>
        /// <value>
        ///  The identifier of the owner.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity OwnerId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema that owns this item.
        /// </summary>
        /// <value>
        ///  The owner schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement OwnerSchema { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property.
        /// </summary>
        /// <value>
        ///  The property.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaProperty Property { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Value { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the conflict value.
        /// </summary>
        /// <value>
        ///  The conflict value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object ConflictValue { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the conflict version.
        /// </summary>
        /// <value>
        ///  The conflict version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long ConflictVersion { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the version.
        /// </summary>
        /// <value>
        ///  The version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long Version { get; private set; }

        internal ConflictException(Identity ownerId, ISchemaElement ownerMetadata, ISchemaProperty property, object value, object conflictValue, long version, long conflictVersion)
        {
            OwnerId = ownerId;
            OwnerSchema = ownerMetadata;
            Property = property;
            Value = value;
            ConflictValue = conflictValue;
            Version = version;
            ConflictVersion = conflictVersion;
        }
    }
}
