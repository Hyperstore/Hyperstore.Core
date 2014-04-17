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

using System.Collections.Generic;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A tracking element.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class TrackingElement
    {
        internal TrackingElement()
        {
            Properties = new Dictionary<string, PropertyValue>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  This is an element of a schema.
        /// </summary>
        /// <value>
        ///  true if this instance is schema, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsSchema { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Version number.
        /// </summary>
        /// <value>
        ///  The version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long Version { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get information about the type of modification.
        /// </summary>
        /// <value>
        ///  The state.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public TrackingState State { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Element id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Schema id of the element.
        /// </summary>
        /// <value>
        ///  The identifier of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Involved properties list.
        /// </summary>
        /// <value>
        ///  The properties.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDictionary<string, PropertyValue> Properties { get; internal set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Link to the involved model element. Null until the session is disposed.
        /// </summary>
        /// <value>
        ///  The model element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement ModelElement { get; internal set; }
    }
}