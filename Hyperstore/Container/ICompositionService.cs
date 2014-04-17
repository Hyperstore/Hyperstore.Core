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
 
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for composition service.
    /// </summary>
    /// <seealso cref="T:IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ICompositionService : IDisposable 
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event handlers in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the event handlers in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<Lazy<IEventHandler, IEventHandlerMetadata>> GetEventHandlers();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the interceptors for domain models in this collection.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the interceptors for domain models in
        ///  this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> GetInterceptorsForDomainModel(IDomainModel domainModel);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the command handlers for domain models in this collection.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the command handlers for domain
        ///  models in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> GetCommandHandlersForDomainModel(IDomainModel domainModel);
    }
}
