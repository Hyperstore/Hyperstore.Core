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
    ///  Info about the value of a property.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class PropertyValue
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Current value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Value { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Current version
        /// </summary>
        /// <value>
        ///  The version.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long CurrentVersion { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Value before the change.
        /// </summary>
        /// <value>
        ///  The old value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object OldValue { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value>
        ///  true if this instance has value, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool HasValue
        {
            get {return CurrentVersion > 0;}
        }
    }
}