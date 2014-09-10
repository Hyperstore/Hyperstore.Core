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
        void NotifyMessages(ISessionInformation session, IExecutionResult log);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send an store closed event.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void NotifyEnd();
    }
}