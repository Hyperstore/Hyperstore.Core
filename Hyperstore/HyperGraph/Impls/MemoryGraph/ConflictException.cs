//-------------------------------------------------------------------------------------------------
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
//-------------------------------------------------------------------------------------------------
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
    public class ConflictException : Exception
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
