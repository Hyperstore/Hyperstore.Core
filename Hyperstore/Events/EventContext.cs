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
 
namespace Hyperstore.Modeling.Events
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An event context.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    ///-------------------------------------------------------------------------------------------------
    public class EventContext<T> where T : class, IEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventContext{T}" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="event">The event.</param>
        internal EventContext(ISessionInformation session, T @event)
        {
            DebugContract.Requires(session);
            DebugContract.Requires(@event);

            Event = @event;
            Session = session;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session.
        /// </summary>
        /// <value>
        ///  The session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISessionInformation Session { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event.
        /// </summary>
        /// <value>
        ///  The event.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public T Event { get; private set; }
    }
}