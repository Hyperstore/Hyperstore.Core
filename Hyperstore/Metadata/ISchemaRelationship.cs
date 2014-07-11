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