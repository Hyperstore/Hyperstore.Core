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
using System.Threading;

#endregion

#if WP8 || PCL
namespace System.Threading.Tasks
{
    internal static class Parallel
    {
        public static void ForEach<TSource>(System.Collections.Generic.IEnumerable<TSource> source, Action<TSource> body)
        {
            foreach (var s in source)
                body(s);
        }
    }
}
#endif

namespace Hyperstore.Modeling.Utils
{
    internal static class ThreadHelper
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current thread identifier.
        /// </summary>
        /// <value>
        ///  The identifier of the current thread.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static int CurrentThreadId
        {
            get
            {
#if NETFX_CORE
                return Environment.CurrentManagedThreadId;
#else
                return Thread.CurrentThread.ManagedThreadId;
#endif
            }
        }

        internal static void Sleep(int ms)
        {
#if NETFX_CORE
            using (ManualResetEvent e = new ManualResetEvent(false))
            {
                e.WaitOne(ms);
            }                
#else
            Thread.Sleep(ms);
#endif
        }
    }
}