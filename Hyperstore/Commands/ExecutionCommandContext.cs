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