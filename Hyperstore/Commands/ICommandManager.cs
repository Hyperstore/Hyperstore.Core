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
        IExecutionResult ProcessCommands(IEnumerable<IDomainCommand> commands);

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