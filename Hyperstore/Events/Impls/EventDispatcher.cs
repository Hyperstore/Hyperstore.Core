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
using System.Linq.Expressions;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Events
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An event dispatcher.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Events.IEventDispatcher"/>
    ///-------------------------------------------------------------------------------------------------
    public class EventDispatcher : IEventDispatcher
    {
        class HandlerInfo
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Name of the domain model.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public string DomainModelName;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  The handler.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public Func<IDomainModel, IEvent, IEnumerable<IDomainCommand>> Handler;
        }

        private readonly Dictionary<Type, List<HandlerInfo>> _handlersByEventType = new Dictionary<Type, List<HandlerInfo>>();
        private readonly bool _initializeWithDefaultHandlers;
        private readonly IHyperstore _store;
        private bool _initialized;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="EventDispatcher" /> class.
        /// </summary>
        /// <param name="services">
        ///  The store.
        /// </param>
        /// <param name="initializeWithDefaultHandlers">
        ///  (Optional) if set to <c>true</c> [initialize with default handlers].
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EventDispatcher(IServicesContainer services, bool initializeWithDefaultHandlers = true)
        {
            Contract.Requires(services, "services");
            _store = services.Resolve<IHyperstore>();
            _initializeWithDefaultHandlers = initializeWithDefaultHandlers;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the event.
        /// </summary>
        /// <param name="event">
        ///  The event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public virtual void HandleEvent(IEvent @event)
        {
            DebugContract.Requires(@event);

            InitializeHandlers();

            var flag = HandleEvent(@event.GetType(), @event);
            flag |= HandleEvent(typeof(IEvent), @event);
            // Si on ne trouve pas de handler et qu'on ne se trouve pas dans la même session qui
            // a généré l'événement, on va quand même notifier l'event 
            if (!flag && @event.CorrelationId != Session.Current.SessionId && !Session.Current.IsDisposing)
            {
                //@event.CorrelationId = Session.Current.SessionId;
                Session.Current.AddEvent(@event);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes the handlers.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void InitializeHandlers()
        {
            if (_initialized)
                return;

            _initialized = true;

#if MEF_NATIVE
            var r = _store.services.Resolve<ICompositionService>();
            if (r != null)
            {
                foreach (var handler in r.GetEventHandlers())
                {
                    Register(handler.Value, handler.Metadata.DomainModel);
                }
            }
#endif

            // TODO prevoir de les surcharger ??
            if (_initializeWithDefaultHandlers)
            {
                Register(new AddEntityEventHandler());
                Register(new AddRelationshipEventHandler());
                Register(new ChangeAttributEventHandler());
                Register(new RemoveEntityEventHandler());
                Register(new RemoveRelationshipEventHandler());

                Register(new AddMetadataEventHandler());
                Register(new AddPropertyMetadataEventHandler());
                Register(new AddRelationshipMetadataEventHandler());
            }
        }

        private bool HandleEvent(Type eventType, IEvent @event)
        {
            Contract.Requires(eventType, "eventType");
            Contract.Requires(@event, "@event");

            List<HandlerInfo> handlers;
            if (!_handlersByEventType.TryGetValue(eventType, out handlers))
                return false;

            IDomainModel targetDomainModel = _store.GetDomainModel(@event.Domain);
            if (targetDomainModel == null)
                return false;

            foreach (var info in handlers)
            {
                if ((info.DomainModelName == null || String.Compare(info.DomainModelName, targetDomainModel.Name, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    var commands = info.Handler(targetDomainModel, @event);
                    Session.Current.Execute(commands.ToArray());
                }
            }

            return handlers.Count > 0;
        }

        #region register

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the specified handler.
        /// </summary>
        /// <param name="handler">
        ///  Objet implémentant les handlers d'evénements.
        /// </param>
        /// <param name="domainModel">
        ///  (Optional) the domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Register(IEventHandler handler, string domainModel = null)
        {
            InitializeHandlers();

            foreach (var invocationTuple in BuildHandlerInvocations(handler))
            {
                AddHandler(invocationTuple.Item1, invocationTuple.Item2, domainModel);
            }
        }

        private void AddHandler(Type type, Func<IDomainModel, IEvent, IEnumerable<IDomainCommand>> handler, string domainModel)
        {
            Contract.Requires(type, "type");
            Contract.Requires(handler, "handler");

            List<HandlerInfo> invocations;
            if (!_handlersByEventType.TryGetValue(type, out invocations))
            {
                invocations = new List<HandlerInfo>();
                _handlersByEventType[type] = invocations;
            }
            invocations.Add(new HandlerInfo { DomainModelName = domainModel, Handler = handler });
        }

        /// <summary>
        ///     Construction des lambdas expressions permettant d'invoker les handlers implémentés par l'objet passé en paramètre
        /// </summary>
        /// <param name="handler">Objet implémentant les handlers d'evénements</param>
        /// <returns></returns>
        private IEnumerable<Tuple<Type, Func<IDomainModel, IEvent, IEnumerable<IDomainCommand>>>> BuildHandlerInvocations(IEventHandler handler)
        {
            Contract.Requires(handler, "handler");

            var interfaces = ReflectionHelper.GetInterfaces(handler.GetType());

            var eventHandlerInvocations = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(IEventHandler<>)))
                    .Select(i => new
                                 {
                                     HandlerInterface = i,
                                     EventType = ReflectionHelper.GetGenericArguments(i)
                                             .First()
                                 })
                    .Select(e => new Tuple<Type, Func<IDomainModel, IEvent, IEnumerable<IDomainCommand>>>(e.EventType, BuildHandlerInvocation(handler, e.HandlerInterface, e.EventType)));
            // Construction des appels typés avec le type de l'event

            return eventHandlerInvocations;
        }

        /// <summary>
        ///     Création dynamique d'un appel vers le handler en forçant le type de l'event
        /// </summary>
        /// <param name="handler">Handler de l'évent</param>
        /// <param name="handlerType">Type de l'interface implémenté par le handler</param>
        /// <param name="eventType">Type de l'event</param>
        /// <returns>Lambda expression</returns>
        private Func<IDomainModel, IEvent, IEnumerable<IDomainCommand>> BuildHandlerInvocation(IEventHandler handler, Type handlerType, Type eventType)
        {
            Contract.Requires(handler, "handler");
            Contract.Requires(handlerType, "handlerType");
            Contract.Requires(eventType, "eventType");

            var pstore = Expression.Parameter(typeof(IDomainModel));
            var pevent = Expression.Parameter(typeof(IEvent));

            var invocationExpression = Expression.Lambda(Expression.Block(Expression.Call(Expression.Convert(Expression.Constant(handler), handlerType), ReflectionHelper.GetMethod(handlerType, "Handle")
                    .First(), pstore, Expression.Convert(pevent, eventType))), pstore, pevent);

            return (Func<IDomainModel, IEvent, IEnumerable<IDomainCommand>>)invocationExpression.Compile();
        }

        #endregion
    }
}