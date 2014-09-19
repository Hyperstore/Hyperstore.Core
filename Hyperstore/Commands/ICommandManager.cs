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

using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for command manager.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface ICommandManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Processes the command.
        /// </summary>
        /// <param name="commands">
        ///  The commands.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISessionResult ProcessCommands(IEnumerable<IDomainCommand> commands);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Processes the command.
        /// </summary>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        ISessionResult ProcessCommands(IDomainCommand command);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers a command interceptor.
        /// </summary>
        /// <typeparam name="TCommand">
        ///  The type of the command.
        /// </typeparam>
        /// <param name="interceptor">
        ///  The interceptor to register.
        /// </param>
        /// <param name="priority">
        ///  (Optional) The priority of the interceptor - Greater value will be executed first.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RegisterInterceptor<TCommand>(ICommandInterceptor<TCommand> interceptor, int priority = 0) where TCommand : IDomainCommand;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers a command handler.
        /// </summary>
        /// <typeparam name="T">
        ///  .
        /// </typeparam>
        /// <param name="handler">
        ///  The handler.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void RegisterCommandHandler<T>(ICommandHandler<T> handler) where T : IDomainCommand;
    }
}