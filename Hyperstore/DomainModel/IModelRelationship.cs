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
    ///  Interface for model relationship.
    /// </summary>
    /// <seealso cref="T:IModelElement"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface IModelRelationship : IModelElement
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema relationship.
        /// </summary>
        /// <value>
        ///  The schema relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ISchemaRelationship SchemaRelationship { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start.
        /// </summary>
        /// <value>
        ///  The start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IModelElement Start { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end.
        /// </summary>
        /// <value>
        ///  The end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IModelElement End { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the end.
        /// </summary>
        /// <value>
        ///  The identifier of the end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity EndId { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the end schema.
        /// </summary>
        /// <value>
        ///  The identifier of the end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity EndSchemaId { get; }
    }
}