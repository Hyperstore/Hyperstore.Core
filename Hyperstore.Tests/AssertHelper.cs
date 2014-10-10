//	Copyright � 2013 - 2014, Alain Metge. All rights reserved.
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
 
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace Xunit
{
    static class AssertHelper
    {
#if NETFX_CORE
        public static T ThrowsException<T>(Action action) where T : Exception
        {
            return Assert.ThrowsException<T>(action);
        }
#else
        //public static T ThrowsException<T>(Action action) where T : Exception
        //{
        //    try
        //    {
        //        action();
        //    }
        //    catch (T ex)
        //    {
        //        return ex;
        //    }

        //    throw new AssertFailedException("Expected exception failed.");
        //}

        //public static async Task<T> ThrowsException<T>(Func<Task> action) where T : Exception
        //{
        //    try
        //    {
        //        await action();
        //    }
        //    catch(T ex)
        //    {
        //        return ex;
        //    }

        //    throw new AssertFailedException("Expected exception failed.");
        //}


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

            Assert.False(reference.IsAlive);
        }
#endif
    }
}
