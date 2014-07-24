using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Events;

namespace Hyperstore.Modeling.Platform.Net
{
    internal class MefContainer : ICompositionService, IMefContainer
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
        [ImportMany(typeof(ICommandInterceptor))]
        public IEnumerable<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> Interceptors { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the event handlers.
        /// </summary>
        /// <value>
        ///  The event handlers.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [ImportMany(typeof(IEventHandler))]
        public IEnumerable<Lazy<IEventHandler, IEventHandlerMetadata>> EventHandlers { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the command handlers.
        /// </summary>
        /// <value>
        ///  The command handlers.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [ImportMany(typeof(ICommandHandler))]
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
