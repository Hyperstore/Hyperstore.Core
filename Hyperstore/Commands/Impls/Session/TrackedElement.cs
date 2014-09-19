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

using System.Collections.Generic;


#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A tracked element.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class TrackedElement
    {
        internal TrackedElement()
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