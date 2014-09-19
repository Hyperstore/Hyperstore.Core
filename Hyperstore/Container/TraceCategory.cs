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
using System.Linq;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A trace category.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class TraceCategory
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  all.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string All = "*";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The domain controler.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string DomainControler = "DomainControler";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The event bus.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string EventBus = "EventBus";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Manager for lock.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string LockManager = "LockManager";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The memory store.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string MemoryStore = "MemoryStore";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The metadata.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string Metadata = "Metadata";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The traverser.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string Traverser = "Traverser";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The hypergraph.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string Hypergraph = "Graph";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The session.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static string Session = "Session";
    }
}