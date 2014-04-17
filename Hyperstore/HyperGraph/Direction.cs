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

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Bitfield of flags for specifying Direction.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum Direction
    {
        /// <summary>
        ///  Specifies the incoming option.
        /// </summary>
        // Default=0,
        Incoming = 1,
        /// <summary>
        ///  Specifies the outgoing option.
        /// </summary>
        Outgoing = 2,
        /// <summary>
        ///  Specifies the both option.
        /// </summary>
        Both = 3
    }
}