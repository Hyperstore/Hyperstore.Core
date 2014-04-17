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
    ///  Values that represent EventPropagationStrategy.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum EventPropagationStrategy
    {
        /// <summary>
        ///  Specifies the none option.
        /// </summary>
        None,
        /// <summary>
        ///  Specifies the top level only option.
        /// </summary>
        TopLevelOnly=1,
        /// <summary>
        ///  Specifies all option.
        /// </summary>
        All=255
    }
}