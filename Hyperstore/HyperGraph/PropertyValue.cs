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