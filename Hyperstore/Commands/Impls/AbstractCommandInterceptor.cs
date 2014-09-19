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
 
using System;
using System.Linq;

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An abstract command interceptor.
    /// </summary>
    /// <typeparam name="TCommand">
    ///  Type of the command.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandInterceptor{TCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class AbstractCommandInterceptor<TCommand> : ICommandInterceptor<TCommand> where TCommand:IDomainCommand 
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [before execution].
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  The BeforeContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual BeforeContinuationStatus OnBeforeExecution(ExecutionCommandContext<TCommand> context)
        {
            return BeforeContinuationStatus.Continue;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [after execution].
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  The ContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ContinuationStatus OnAfterExecution(ExecutionCommandContext<TCommand> context)
        {
            return ContinuationStatus.Continue;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [error].
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <param name="exception">
        ///  The exception.
        /// </param>
        /// <returns>
        ///  The ErrorContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ErrorContinuationStatus OnError(ExecutionCommandContext<TCommand> context, Exception exception)
        {
            return ErrorContinuationStatus.Abort;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether [is applicable on] [the specified store].
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  true if applicable on, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual bool IsApplicableOn(ISession session, TCommand command)
        {
            return true;
        }
    }
}
