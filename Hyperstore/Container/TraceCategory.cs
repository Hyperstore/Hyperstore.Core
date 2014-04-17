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