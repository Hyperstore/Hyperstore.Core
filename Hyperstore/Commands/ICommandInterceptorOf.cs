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

using System;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Command interceptor.
    /// </summary>
    /// <typeparam name="TCommand">
    ///  .
    /// </typeparam>
    /// <seealso cref="T:ICommandInterceptor"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface ICommandInterceptor<TCommand> : ICommandInterceptor where TCommand : IDomainCommand
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called before a command is executed.
        /// </summary>
        /// <param name="context">
        ///  The command context.
        /// </param>
        /// <returns>
        ///  Values possibles are Continue, Abort and SkipCommand.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        BeforeContinuationStatus OnBeforeExecution(ExecutionCommandContext<TCommand> context);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called after a command has been executed correctly.
        /// </summary>
        /// <param name="context">
        ///  The command context.
        /// </param>
        /// <returns>
        ///  Values possibles are Continue and Abort.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ContinuationStatus OnAfterExecution(ExecutionCommandContext<TCommand> context);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when an error occurs during the execution of a command.
        /// </summary>
        /// <param name="context">
        ///  The command context.
        /// </param>
        /// <param name="exception">
        ///  The exception raised.
        /// </param>
        /// <returns>
        ///  Values possibles are Continue, Abort and Retry.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ErrorContinuationStatus OnError(ExecutionCommandContext<TCommand> context, Exception exception);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether [is applicable on] [the specified command].
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  <c>true</c> if [is applicable on] [the specified command]; otherwise, <c>false</c>.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsApplicableOn(ISession session, TCommand command);
    }
}