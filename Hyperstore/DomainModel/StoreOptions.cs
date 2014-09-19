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
 
//-------------------------------------------------------------------------------------------------
// file:	DomainModel\StoreOptions.cs
//
// summary:	Implements the store options class
//-------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Specifying Store Options.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum StoreOptions
    {
        /// <summary>
        ///  No option
        /// </summary>
        None = 0,
        /// <summary>
        ///  Store supports scoping for domain model
        /// </summary>
        EnableScopings = 1
    }
}
