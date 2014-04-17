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
 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    static class AssertHelper
    {
#if NETFX_CORE
        public static T ThrowsException<T>(Action action) where T : Exception
        {
            return Assert.ThrowsException<T>(action);
        }
#else
        public static T ThrowsException<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T ex)
            {
                return ex;
            }

            throw new AssertFailedException("Expected exception failed.");
        }

        public static async Task<T> ThrowsException<T>(Func<Task> action) where T : Exception
        {
            try
            {
                await action();
            }
            catch(T ex)
            {
                return ex;
            }

            throw new AssertFailedException("Expected exception failed.");
        }


        public static void IsGarbageCollected<TObject>(ref TObject @object)    where TObject : class
        {
            Action<TObject> emptyAction = o => { };
            IsGarbageCollected(ref @object, emptyAction);
        }

        public static void IsGarbageCollected<TObject>( ref TObject @object, Action<TObject> useObject) where TObject : class
        {
            if (typeof(TObject) == typeof(string))
            {
                // Strings are copied by value, and don't leak anyhow.
                return;
            }

            int generation = GC.GetGeneration(@object);
            useObject(@object);
            WeakReference reference = new WeakReference(@object);
            @object = null;

            GC.Collect(generation, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            Assert.IsFalse(reference.IsAlive);
        }
#endif
    }
}
