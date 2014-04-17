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
