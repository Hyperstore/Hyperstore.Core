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
