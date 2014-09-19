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
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for events processor.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventsProcessor
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Process the events.
        /// </summary>
        /// <param name="origin">
        ///  The origin.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        /// <param name="events">
        ///  The events.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void ProcessEvents(Guid origin, SessionMode mode, IEnumerable<IEvent> events);
    }
}