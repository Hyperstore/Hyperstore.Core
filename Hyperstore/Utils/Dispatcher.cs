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
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Utils
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An empty dispatcher.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISynchronizationContext"/>
    ///-------------------------------------------------------------------------------------------------
    public class EmptyDispatcher : Hyperstore.Modeling.ISynchronizationContext
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send this message.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Task Send(Action action)
        {
            action();
            return CompletedTask.Default;
        }
    }

#if NET45

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A dispatcher in a UI context
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISynchronizationContext"/>
    ///-------------------------------------------------------------------------------------------------
    public class UIDispatcher : Hyperstore.Modeling.ISynchronizationContext
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public UIDispatcher()
        {
            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send this message.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public async Task Send(Action action)
        {
            if (_dispatcher == null)
                throw new Exception("Incorrect UI dispatcher for the context of the current application. Redefines the correct dispatcher in the store.");

            if (_dispatcher.CheckAccess())
            {
                action();
                await CompletedTask.Default;
            }
            else
                await _dispatcher.InvokeAsync(action, System.Windows.Threading.DispatcherPriority.Normal);
        }
    }
#elif WIN8
    public class UIDispatcher : Hyperstore.Modeling.ISynchronizationContext
    {
        private Windows.UI.Core.CoreDispatcher _dispatcher;
       
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public UIDispatcher()
        {
            _dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send this message.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public async Task Send(Action action)
        {
            if (_dispatcher == null)
                throw new Exception("Incorrect UI dispatcher for the context of the current application. Redefines the correct dispatcher in the store.");
    
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(action));
        }
    }
#elif WP8
        public class UIDispatcher : Hyperstore.Modeling.ISynchronizationContext
    {
        private Windows.UI.Core.CoreDispatcher _dispatcher;
       
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public UIDispatcher()
        {
            _dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send this message.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public async Task Send(Action action)
        {
            if (_dispatcher == null)
                throw new Exception("Incorrect UI dispatcher for the context of the current application. Redefines the correct dispatcher in the store.");
    
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(action));
        }
    }
#endif
}
