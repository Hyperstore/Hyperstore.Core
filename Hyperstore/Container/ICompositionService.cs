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
 
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Metadata.Constraints;
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
    internal interface ICompositionService : IDisposable 
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event handlers in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the event handlers in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<Lazy<IEventHandler, ICompositionMetadata>> GetEventHandlers();

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
        IEnumerable<Lazy<ICommandHandler, ICompositionMetadata>> GetCommandHandlersForDomainModel(IDomainModel domainModel);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the constraints for domain models in this collection.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the constraints for domain models in
        ///  this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<Lazy<IConstraint, ICompositionMetadata>> GetConstraintsForDomainModel(IDomainModel domainModel);
    }
}
