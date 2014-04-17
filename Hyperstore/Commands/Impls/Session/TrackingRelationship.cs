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
 
namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A tracking relationship.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.TrackingElement"/>
    ///-------------------------------------------------------------------------------------------------
    public class TrackingRelationship : TrackingElement
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the start.
        /// </summary>
        /// <value>
        ///  The identifier of the start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartId { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the start schema.
        /// </summary>
        /// <value>
        ///  The identifier of the start schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity StartSchemaId { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the end.
        /// </summary>
        /// <value>
        ///  The identifier of the end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndId { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier of the end schema.
        /// </summary>
        /// <value>
        ///  The identifier of the end schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity EndSchemaId { get; internal set; }
    }
}