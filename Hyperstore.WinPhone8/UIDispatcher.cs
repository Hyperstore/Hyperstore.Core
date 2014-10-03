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
using System.Threading.Tasks;
using System.Windows;

namespace Hyperstore.Modeling.Platform.WinPhone
{
    internal class UIDispatcher : Hyperstore.Modeling.ISynchronizationContext
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public UIDispatcher()
        {
            _dispatcher = Deployment.Current.Dispatcher;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send this message.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public Task Send(Action action)
        {
            if (_dispatcher == null)
                throw new HyperstoreException("Incorrect UI dispatcher for the context of the current application. Redefines the correct dispatcher in the store.");

            if (_dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _dispatcher.BeginInvoke(action);    
            }
            
            return Hyperstore.Modeling.Utils.CompletedTask.Default;
        }
    }
}
