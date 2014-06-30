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
