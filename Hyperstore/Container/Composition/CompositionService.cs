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
