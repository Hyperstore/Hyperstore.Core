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
using Hyperstore.Modeling.Metadata.Constraints;
#endregion

namespace Hyperstore.Modeling.Container.Composition
{
    internal class CompositionContainer : ICompositionService
    {
        private readonly List<Lazy<ICommandHandler, ICompositionMetadata>> _commands = new List<Lazy<ICommandHandler, ICompositionMetadata>>();
        private readonly List<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> _interceptors = new List<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>>();
        private readonly List<Lazy<IEventHandler, ICompositionMetadata>> _eventHandlers = new List<Lazy<IEventHandler, ICompositionMetadata>>();
        private readonly List<Lazy<IConstraint, ICompositionMetadata>> _constraints = new List<Lazy<IConstraint, ICompositionMetadata>>();


        public void Compose(params Assembly[] assemblies)
        {            
            var types = assemblies
                        .SelectMany(asm => asm.DefinedTypes);

            foreach (var typeInfo in types)
            {
                if (typeInfo.IsAbstract)
                    continue;

                foreach (var attr in typeInfo.GetCustomAttributes(true))
                {
                    if (attr is CommandHandlerAttribute)
                    {
                        _commands.Add(new Lazy<ICommandHandler, ICompositionMetadata>(() => (ICommandHandler)Activator.CreateInstance(typeInfo.AsType()), (ICompositionMetadata)attr));
                    }
                    else if (attr is CommandInterceptorAttribute)
                    {
                        _interceptors.Add(new Lazy<ICommandInterceptor, ICommandInterceptorMetadata>(() => (ICommandInterceptor)Activator.CreateInstance(typeInfo.AsType()), (ICommandInterceptorMetadata)attr));
                    }
                    else if (attr is EventHandlerAttribute)
                    {
                        _eventHandlers.Add(new Lazy<IEventHandler, ICompositionMetadata>(() => (IEventHandler)Activator.CreateInstance(typeInfo.AsType()), (ICompositionMetadata)attr));
                    }
                    else if(attr is ConstraintAttribute)
                    {
                        _constraints.Add(new Lazy<IConstraint, ICompositionMetadata>(() => (IConstraint)Activator.CreateInstance(typeInfo.AsType()), (ICompositionMetadata)attr));
                    }
                }
            }
        }

        IEnumerable<Lazy<IConstraint, ICompositionMetadata>> ICompositionService.GetConstraintsForDomainModel(IDomainModel domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            foreach (var constraint in _constraints)
            {
                if ((constraint.Metadata.DomainModel == null || String.Compare(constraint.Metadata.DomainModel, domainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                    yield return constraint;
            }
        }


        IEnumerable<Lazy<ICommandHandler, ICompositionMetadata>> ICompositionService.GetCommandHandlersForDomainModel(IDomainModel domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            foreach (var handler in _commands)
            {
                if ((handler.Metadata.DomainModel == null || String.Compare(handler.Metadata.DomainModel, domainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                    yield return handler;
            }
        }

        IEnumerable<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> ICompositionService.GetInterceptorsForDomainModel(IDomainModel domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            foreach (var rule in _interceptors)
            {
                if ((rule.Metadata.DomainModel == null || String.Compare(rule.Metadata.DomainModel, domainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                    yield return rule;
            }
        }

        IEnumerable<Lazy<IEventHandler, ICompositionMetadata>> ICompositionService.GetEventHandlers()
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
