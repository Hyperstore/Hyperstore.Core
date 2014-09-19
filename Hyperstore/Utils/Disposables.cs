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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling.Utils
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A disposables.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class Disposables
    {
        private static readonly IDisposable _empty;

        static Disposables()
        {
            _empty = new EmptyDisposable();
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private sealed class AnonymousDisposable : IDisposable
        {
            private readonly Action action;

            public AnonymousDisposable(Action action)
            {
                Contract.Requires(action, "action");
                this.action = action;
            }

            public void Dispose()
            {
                try
                {
                    action();
                }
                catch 
                {
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Return an empty disposable
        /// </summary>
        /// <value>
        ///  An idisposable which does nothing
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static IDisposable Empty
        {
            get { return _empty; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the specified action on dispose.
        /// </summary>
        /// <param name="action">
        ///  The action to execute (Exceptions are catched)
        /// </param>
        /// <returns>
        ///  An IDisposable which executes the specified action
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IDisposable ExecuteOnDispose(Action action)
        {
            return new AnonymousDisposable(action);
        }
    }
}
