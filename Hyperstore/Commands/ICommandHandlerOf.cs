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

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for command handler.
    /// </summary>
    /// <typeparam name="TCommand">
    ///  Type of the command.
    /// </typeparam>
    /// <seealso cref="T:ICommandHandler"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface ICommandHandler<TCommand> : ICommandHandler where TCommand : IDomainCommand
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the specified context.
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An IEvent.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEvent Handle(ExecutionCommandContext<TCommand> context);
    }
}