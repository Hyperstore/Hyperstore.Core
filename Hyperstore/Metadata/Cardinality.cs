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

using System;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Bitfield of flags for specifying Cardinality.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    [Flags]
    public enum Cardinality
    {
        /// <summary>
        ///  Specifies the one to one= 0 option.
        /// </summary>
        OneToOne=0,

        /// <summary>
        ///  Specifies the one to many= 1 option.
        /// </summary>
        OneToMany=1,

        /// <summary>
        ///  Specifies the many to one= 2 option.
        /// </summary>
        ManyToOne=2,

        /// <summary>
        ///  Specifies the many to many= 3 option.
        /// </summary>
        ManyToMany=3
    }
}