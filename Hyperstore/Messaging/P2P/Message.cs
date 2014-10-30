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
using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A message.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class Message
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the mode.
        /// </summary>
        /// <value>
        ///  The mode.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionMode sessionMode { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the origin store.
        /// </summary>
        /// <value>
        ///  The identifier of the origin store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string origin { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the events.
        /// </summary>
        /// <value>
        ///  The events.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public List<Enveloppe> events { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the session.
        /// </summary>
        /// <value>
        ///  The identifier of the session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int sessionId { get; set; }
    }
}