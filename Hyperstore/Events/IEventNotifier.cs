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

using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for event notifier.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IEventNotifier
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Notifies the event.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="log">
        ///  The log.
        /// </param>
        /// <param name="ev">
        ///  The ev.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void NotifyEvent(ISessionInformation session, ISessionContext log, IEvent ev);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send events when the session end (correctly or not)
        /// </summary>
        /// <param name="session">
        ///  .
        /// </param>
        /// <param name="log">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void NotifySessionCompleted(ISessionInformation session, ISessionContext log);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send all validations message raises during the session.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="log">
        ///  The log.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void NotifyMessages(ISessionInformation session, ISessionResult log);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send an store closed event.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void NotifyEnd();
    }
}