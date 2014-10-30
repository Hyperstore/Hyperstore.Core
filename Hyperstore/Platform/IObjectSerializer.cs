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
 
using System;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for object serializer.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IJsonSerializer
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Deserialize a json string
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        /// <param name="defaultValue">
        ///  The default value.
        /// </param>
        /// <param name="obj">
        ///  (Optional) the object.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        object Deserialize(string data, object defaultValue, object obj = null);

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
