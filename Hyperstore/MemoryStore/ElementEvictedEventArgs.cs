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

using System;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Additional information for element evicted events.
    /// </summary>
    /// <seealso cref="T:System.EventArgs"/>
    ///-------------------------------------------------------------------------------------------------
    public class ElementEvictedEventArgs : EventArgs
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="status">
        ///  The status.
        /// </param>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ElementEvictedEventArgs(EvictionProcess status, object id)
        {
            Status = status;
            Id = id;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Id { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the status.
        /// </summary>
        /// <value>
        ///  The status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public EvictionProcess Status { get; private set; }
    }
}