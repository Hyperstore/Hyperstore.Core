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

using Hyperstore.Modeling.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform.Net
{

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A dispatcher in a UI context
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ISynchronizationContext"/>
    ///-------------------------------------------------------------------------------------------------
    public class UIDispatcher : global::Hyperstore.Modeling.ISynchronizationContext
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
            if (_dispatcher == null || _dispatcher.CheckAccess())
            {
                action();
                await CompletedTask.Default;
            }
            else
                await _dispatcher.InvokeAsync(action, System.Windows.Threading.DispatcherPriority.Normal);
        }
    }

}
