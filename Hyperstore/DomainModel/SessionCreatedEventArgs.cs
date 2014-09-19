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
 
#region Imports

using System;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Additional information for session created events.
    /// </summary>
    /// <seealso cref="T:System.EventArgs"/>
    ///-------------------------------------------------------------------------------------------------
    public class SessionCreatedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionCreatedEventArgs" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        internal SessionCreatedEventArgs(ISession session)
        {
            Contract.Requires(session, "session");
            Session = session;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the session.
        /// </summary>
        /// <value>
        ///  The session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISession Session { get; set; }
    }
}