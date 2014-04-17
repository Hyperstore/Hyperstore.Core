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

#if MEF_NATIVE
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;


namespace Hyperstore.Modeling.Ioc
{
    internal class MefContainer : ICompositionService
    {
        private CompositionContainer _container;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Composes the given assemblies.
        /// </summary>
        /// <param name="assemblies">
        ///  A variable-length parameters list containing assemblies.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Compose(params Assembly[] assemblies)
        {
            DoComposition(assemblies);
        }

#pragma warning disable 0649

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the interceptors.
        /// </summary>
        /// <value>
        ///  The interceptors.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [ImportMany(typeof(Hyperstore.Modeling.Commands.ICommandInterceptor))]
        public IEnumerable<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> Interceptors { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the event handlers.
        /// </summary>
        /// <value>
        ///  The event handlers.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [ImportMany(typeof(Hyperstore.Modeling.Events.IEventHandler))]
        public IEnumerable<Lazy<IEventHandler, IEventHandlerMetadata>> EventHandlers { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the command handlers.
        /// </summary>
        /// <value>
        ///  The command handlers.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [ImportMany(typeof(Hyperstore.Modeling.Commands.ICommandHandler))]
        public IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> CommandHandlers { get; set; }
#pragma warning restore 0649

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the composition operation.
        /// </summary>
        /// <param name="assemblies">
        ///  A variable-length parameters list containing assemblies.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void DoComposition(params Assembly[] assemblies)
        {
            var catalog = new AggregateCatalog(assemblies.Select(a => new AssemblyCatalog(a)));
            _container = new CompositionContainer(catalog);
            _container.ComposeParts(this);
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
            if (CommandHandlers != null)
            {
                foreach (var handler in CommandHandlers)
                {
                    if ((handler.Metadata.DomainModel == null || String.Compare(handler.Metadata.DomainModel, domainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                        yield return handler;
                }
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
            if (Interceptors != null)
            {
                foreach (var rule in Interceptors)
                {
                    if ((rule.Metadata.DomainModel == null || String.Compare(rule.Metadata.DomainModel, domainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                        yield return rule;
                }
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
            if (EventHandlers != null)
            {
                foreach (var handler in EventHandlers)
                {
                    yield return handler;
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (_container != null)
                _container.Dispose();
        }
    }
}

#else
namespace Hyperstore.Modeling
{
    //http://mef.codeplex.com/wikipage?title=MetroChanges
    // TODO a finir
    class MefContainer : ICompositionService
    {
        public MefContainer(params Assembly[] assemblies)
        {
 //App.CompositionHost.SatisfyImports(this);
        }

    //internal static CompositionHost CompositionHost
    //{
    //    get
    //    {
    //        return _compositionHost ?? (_compositionHost = new ContainerConfiguration()
    //            .WithAssembly(System.Reflection.Assembly.GetExecutingAssembly())
    //            .CreateContainer());
    //    }
    //}
        public void Compose(params Assembly[] assemblies)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> GetInterceptorsForDomainModel(IDomainModel domainModel)
        {
            yield break;
        }
        
        public IEnumerable<Lazy<IEventHandler, IEventHandlerMetadata>> GetEventHandlers()
        {
            yield break;
        }

        public IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> GetCommandHandlersForDomainModel(IDomainModel domainModel)
        {
            yield break;
        }

        public void Dispose()
        {
        }
    }
}

#endif