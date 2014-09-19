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