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

namespace Hyperstore.Modeling.Platform
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for object serializer.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IObjectSerializer
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="implementationType">
        ///  Type of the implementation.
        /// </param>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <param name="defaultValue">
        ///  The default value.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        object Deserialize(Type implementationType, string data, object defaultValue);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A T.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T Deserialize<T>(string data);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        string Serialize(object data);
    }
}
