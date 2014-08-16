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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Events;
#endregion

namespace Hyperstore.Modeling.Container.Composition
{
    internal class CompositionContainer : ICompositionService
    {
        private readonly List<Lazy<ICommandHandler, ICommandHandlerMetadata>> _commands = new List<Lazy<ICommandHandler, ICommandHandlerMetadata>>();
        private readonly List<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> _interceptors = new List<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>>();
        private readonly List<Lazy<IEventHandler, IEventHandlerMetadata>> _eventHandlers = new List<Lazy<IEventHandler, IEventHandlerMetadata>>();

        public void Compose(params Assembly[] assemblies)
        {            
            var types = assemblies
                        .SelectMany(asm => asm.ExportedTypes);

            foreach (var type in types)
            {
                var typeInfo = type.GetTypeInfo();
                if (!typeInfo.IsPublic)
                    continue;

                foreach (var attr in typeInfo.GetCustomAttributes(true))
                {
                    if (attr is CommandHandlerAttribute)
                    {
                        _commands.Add(new Lazy<ICommandHandler, ICommandHandlerMetadata>( () => (ICommandHandler)Activator.CreateInstance(type), (ICommandHandlerMetadata)attr));
                    }
                    else if (attr is CommandInterceptorAttribute)
                    {
                        _interceptors.Add(new Lazy<ICommandInterceptor, ICommandInterceptorMetadata>(() => (ICommandInterceptor)Activator.CreateInstance(type), (ICommandInterceptorMetadata)attr));
                    }
                    else if (attr is EventHandlerAttribute)
                    {
                        _eventHandlers.Add(new Lazy<IEventHandler, IEventHandlerMetadata>(() => (IEventHandler)Activator.CreateInstance(type), (IEventHandlerMetadata)attr));
                    }
                }
            }
        }

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
        public IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> GetCommandHandlersForDomainModel(IDomainModel domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            foreach (var handler in _commands)
            {
                if ((handler.Metadata.DomainModel == null || String.Compare(handler.Metadata.DomainModel, domainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                    yield return handler;
            }
        }

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
        public IEnumerable<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> GetInterceptorsForDomainModel(IDomainModel domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            foreach (var rule in _interceptors)
            {
                if ((rule.Metadata.DomainModel == null || String.Compare(rule.Metadata.DomainModel, domainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                    yield return rule;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the event handlers in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the event handlers in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<Lazy<IEventHandler, IEventHandlerMetadata>> GetEventHandlers()
        {
            foreach (var handler in _eventHandlers)
            {
                yield return handler;
            }
        }

        public void Dispose()
        {
        }
    }
}
