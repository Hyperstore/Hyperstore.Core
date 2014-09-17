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

#endregion

#region Imports

using System;
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Execution context of a command.
    /// </summary>
    /// <typeparam name="TCommand">
    ///  .
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.ISessionContext"/>
    ///-------------------------------------------------------------------------------------------------
    public sealed class ExecutionCommandContext<TCommand> : ISessionContext where TCommand : IDomainCommand
    {
        #region Fields

        private readonly ISessionContext _log;

        #endregion

        #region Properties

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the command.
        /// </summary>
        /// <value>
        ///  The command.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public TCommand Command { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the current session.
        /// </summary>
        /// <value>
        ///  The current session.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISession CurrentSession { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the message list.
        /// </summary>
        /// <value>
        ///  The message list.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISessionResult Result
        {
            get { return _log.Result; }
        }

        internal ISessionContext Context
        {
            get { return _log; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExecutionCommandContext{TCommand}" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="log">The log.</param>
        /// <param name="command">The command.</param>
        internal ExecutionCommandContext(ISession session, ISessionContext log, TCommand command)
        {
            DebugContract.Requires(session, "session");
            DebugContract.Requires(log, "log");
            DebugContract.Requires(command != null, "command");

            CurrentSession = session;
            Command = command;
            _log = log;
        }

        #endregion

        #region Methods

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Logs a message.
        /// </summary>
        /// <param name="message">
        ///  The message.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Log(DiagnosticMessage message)
        {
            Contract.Requires(message, "message");
            _log.Log(message);
        }

        /// <summary>
        ///     Adds the event.
        /// </summary>
        /// <param name="event">The event.</param>
        internal void AddEvent(IEvent @event)
        {
            DebugContract.Requires(@event);

            CurrentSession.AddEvent(@event);
        }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <returns></returns>
        internal IEvent ExecuteCommand()
        {
            if (Command is ICommandHandler<TCommand>)
                return ((ICommandHandler<TCommand>) Command).Handle(this);

            return null;
        }

        #endregion
    }
}