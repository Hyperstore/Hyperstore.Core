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
 
#region Imports

using System;
using Hyperstore.Modeling.Commands;
using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.Adapters
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for persistence graph adapter.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface IPersistenceGraphAdapter : IGraphAdapter
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Persist session elements.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="elementsToPersist">
        ///  The elements to persist.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void PersistElements(ISessionInformation session, IEnumerable<TrackedElement> elementsToPersist);
    }
}