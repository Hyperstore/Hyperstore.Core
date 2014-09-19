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