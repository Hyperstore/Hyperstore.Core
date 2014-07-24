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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Commands
{
    /// <summary>
    ///     Command manager
    /// </summary>
    /// <remarks>
    ///     Il existe un command manager par domaine. Chaque commande va possèder un CommandProcessor chargé d'exécuter le
    ///     handler de la commande
    ///     dans un pipeline contenant les intercepteurs.
    /// </remarks>
    internal sealed class CommandManager : ICommandManager, IDomainService, IDisposable
    {
        // Stockage de l'appel typé du processor de la commande.
        // L'implémentation du processor utilise une méthode acceptant une commande typée (Méthode générique)
        // Ceci nécessite de générer l'appel dynamiquement à partir du type fourni en paramètre.
        // La génération s'effectue une seule fois dans la méthode CreateProcessor
        /// <summary>
        ///     Liste des processors par commande
        /// </summary>
        private readonly Dictionary<string, CommandProcessorInfo> _commandProcessors = new Dictionary<string, CommandProcessorInfo>();
        private IDomainModel _domainModel;
        /// <summary>
        /// Liste des intercepteurs par type
        /// </summary>
        private readonly Dictionary<Type, List<InterceptorInfo>> _interceptors = new Dictionary<Type, List<InterceptorInfo>>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers a command handler.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="handler">
        ///  The handler.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterCommandHandler<T>(ICommandHandler<T> handler) where T : IDomainCommand
        {
            Contract.Requires(handler, "handler");
            var processInfo = GetCommandProcessor(typeof(T));
            processInfo.Processor.SetHandler(handler);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enregistrement d'une règle (after/before) associée à une commande.
        /// </summary>
        /// <typeparam name="T">
        ///  Type de la commande.
        /// </typeparam>
        /// <param name="interceptor">
        ///  Régle à exécuter.
        /// </param>
        /// <param name="priority">
        ///  (Optional) Priorité d'éxécution (ascendant) - Default is 0.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterInterceptor<T>(ICommandInterceptor<T> interceptor, int priority = 0) where T : IDomainCommand
        {
            Contract.Requires(interceptor, "interceptor");

            RegisterCommandInterceptor(typeof(T), interceptor, priority);
        }

        private void RegisterCommandInterceptor(Type commandRuleType, ICommandInterceptor interceptor, int priority = 0)
        {
            DebugContract.Requires(interceptor != null);
            List<InterceptorInfo> list;

            if (!_interceptors.TryGetValue(commandRuleType, out list))
            {
                list = new List<InterceptorInfo>();
                _interceptors.Add(commandRuleType, list);
            }
            list.Add(new InterceptorInfo { Priority = priority, Interceptor = interceptor, CommandType = commandRuleType });

            foreach (var item in _commandProcessors)
            {
                var commandType = item.Value.CommandType; // Command type
                if (ReflectionHelper.IsAssignableFrom(commandRuleType, commandType))
                    item.Value.IsPrepared = false;
            }
        }

        void IDisposable.Dispose()
        {
            _commandProcessors.Clear();
            GC.SuppressFinalize(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Execution des commandes atomiquement. Une exception est générée en cas d'erreur.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <exception cref="SessionException">
        ///  Thrown when a Session error condition occurs.
        /// </exception>
        /// <param name="commands">
        ///  .
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IExecutionResult ICommandManager.ProcessCommands(IEnumerable<IDomainCommand> commands)
        {
            Contract.Requires(commands != null, "commands");

            var session = Session.Current;
            if (session == null)
                throw new SessionRequiredException();

            var log = session as ISessionContext;
            Debug.Assert(log != null, "log != null");

            // Execution des commandes
            foreach (var cmd in commands)
            {
                if (Session.Current.CancellationToken.IsCancellationRequested)
                    break;

                var info = GetCommandProcessor(cmd.GetType());
                Debug.Assert(info != null);
                PrepareProcessor(info);
                var status = ContinuationStatus.Abort;
                try
                {
                    status = info.RunProcessor(session, log, cmd);
                    if (status == ContinuationStatus.Abort)
                    {
                        log.Log(new DiagnosticMessage(MessageType.Error, ExceptionMessages.Diagnostic_CommandAborted, "Command"));
                    }
                }
                catch (Exception ex) // Si bug ds Hyperstore ;-)
                {
                    log.Log(new DiagnosticMessage(MessageType.Error, ExceptionMessages.Diagnostic_CommandAborted, "Fatal error", ex: ex));
                }

                if (status == ContinuationStatus.Abort)
                {
                    throw new SessionException(log.Result.Messages);
                }
            }
            return log.Result;
        }

        private void PrepareProcessor(CommandProcessorInfo processorInfo)
        {
            if (processorInfo.IsPrepared)
                return;

            processorInfo.Processor.ClearInterceptors();
            foreach (var item in _interceptors)
            {
                var ruleCommandType = item.Key;
                if (!ReflectionHelper.IsAssignableFrom(ruleCommandType, processorInfo.CommandType))
                    continue;

                foreach (var info in item.Value)
                {
                    var i = info.CommandType == processorInfo.CommandType ? info.Interceptor : new InterceptorWrapper(info.CommandType, info.Interceptor);
                    processorInfo.Processor.RegisterInterceptor(i, info.Priority);
                }
                processorInfo.IsPrepared = true;
            }
        }

        /// <summary>
        ///     Initialisation du service avec le domaine associé. Cette méthode est appelée quand le
        ///     service est instancié par le domaine
        /// </summary>
        /// <param name="domainModel">Domaine associé</param>
        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);
            _domainModel = domainModel;
            RegisterFromComposition(domainModel);
        }

        /// <summary>
        ///     Registers the rules defined via MEF
        /// </summary>
        /// <param name="domainModel">The domain model.</param>
        private void RegisterFromComposition(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel != null);
            var compositionService = domainModel.Store.DependencyResolver.Resolve<ICompositionService>();
            if (compositionService == null)
                return;

            foreach (var handler in compositionService.GetCommandHandlersForDomainModel(domainModel))
            {
                var ruleType = handler.Value.GetType();
                var interfaces = ReflectionHelper.GetInterfaces(ruleType);

                var commandRuleType = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(ICommandHandler<>)))
                                                .Select(i => new { RuleType = i, CommandType = ReflectionHelper.GetGenericArguments(i).First() })
                                                .FirstOrDefault();
                if (commandRuleType == null)
                    continue;

                var processInfo = GetCommandProcessor(commandRuleType.CommandType);
                processInfo.Processor.SetHandler(handler.Value);
            }

            foreach (var rule in compositionService.GetInterceptorsForDomainModel(domainModel))
            {
                var ruleType = rule.Value.GetType();
                var interfaces = ReflectionHelper.GetInterfaces(ruleType);
                var commandRuleType = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(ICommandInterceptor<>)))
                                .Select(i => new { RuleType = i, CommandType = ReflectionHelper.GetGenericArguments(i).First() })
                                .FirstOrDefault();
                if (commandRuleType == null)
                    continue;
                RegisterCommandInterceptor(commandRuleType.CommandType, rule.Value, rule.Metadata.Priority);
            }
        }

        /// <summary>
        ///     Recherche du processor associé à la commande (Création si il n'existe pas)
        ///     Cet appel est thread-safe
        /// </summary>
        /// <param name="commandType">Type de la commande</param>
        /// <returns></returns>
        private CommandProcessorInfo GetCommandProcessor(Type commandType)
        {
            DebugContract.Requires(commandType);

            CommandProcessorInfo info;
            var key = commandType.FullName;
            if (!_commandProcessors.TryGetValue(key, out info))
            {
                lock (_commandProcessors)
                {
                    if (!_commandProcessors.TryGetValue(key, out info))
                    {
                        // Defaut processor pour le type 'commandtype'
                        info = new CommandProcessorInfo();
                        info.RunProcessor = CreateProcessor(commandType, info);
                        _commandProcessors.Add(key, info);
                    }
                }
            }

            return info;
        }

        /// <summary>
        ///     Création de l'appel du processor. Consiste à générer une appel générique typé à partir du type fourni
        /// </summary>
        /// <param name="commandType">Type de la commande</param>
        /// <param name="info"></param>
        /// <returns></returns>
        private Func<ISession, ISessionContext, IDomainCommand, ContinuationStatus> CreateProcessor(Type commandType, CommandProcessorInfo info)
        {
            DebugContract.Requires(commandType);
            DebugContract.Requires(info);

            // p = new CommandProcessor<commandtype>()
            var processorType = typeof(CommandProcessor<>).MakeGenericType(commandType);
            info.Processor = Activator.CreateInstance(processorType, _domainModel.Store) as ICommandProcessor;
            info.CommandType = commandType;

            // new ExecutionCommandContext<commandType>(s,c)
            var contextType = typeof(ExecutionCommandContext<>).MakeGenericType(commandType); // Création du type générique voulu
            // Recherche du constructeur de ce nouveau type pour pouvoir l'instancier
            var contextCtor = ReflectionHelper.GetProtectedConstructor(contextType, new[] { typeof(ISession), typeof(ISessionContext), commandType });

            // Génération en dynamique d'un appel en utilisant un contexte typé
            var psession = Expression.Parameter(typeof(ISession));
            var pcommand = Expression.Parameter(typeof(IDomainCommand));
            var plog = Expression.Parameter(typeof(ISessionContext));

            // p.RunProcessor = (s, log, c) => p.Process(new ExecutionCommandContext<commandType>(s,log, c))
            var invocationExpression = Expression.Lambda(Expression.Block(Expression.Call(Expression.Convert(Expression.Constant(info.Processor), processorType), // p
                    ReflectionHelper.GetMethod(processorType, "Process")
                            .First(), // .Process
                    Expression.New(contextCtor, psession, plog, Expression.Convert(pcommand, commandType)) // new ExecutionCommandContext<commandType>(s,log, c)
                    )), psession, plog, pcommand); // (s, log, c)

            return (Func<ISession, ISessionContext, IDomainCommand, ContinuationStatus>)invocationExpression.Compile();
        }

        private class CommandProcessorInfo
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Méthode lambda permettant d'appeler le processor (s, c, log) => p.Process(new
            ///  ExecutionCommandContext&lt;commandType&gt;(s,log, c))
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public Func<ISession, ISessionContext, IDomainCommand, ContinuationStatus> RunProcessor;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Processor associé à la commande.
            /// </summary>
            /// <value>
            ///  The processor.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public ICommandProcessor Processor { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets or sets the type of the command.
            /// </summary>
            /// <value>
            ///  The type of the command.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public Type CommandType { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets or sets a value indicating whether this instance is prepared.
            /// </summary>
            /// <value>
            ///  true if this instance is prepared, false if not.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public bool IsPrepared { get; set; }
        }

        private struct InterceptorInfo
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets or sets the priority.
            /// </summary>
            /// <value>
            ///  The priority.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public int Priority { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets or sets the interceptor.
            /// </summary>
            /// <value>
            ///  The interceptor.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public ICommandInterceptor Interceptor { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets or sets the type of the command.
            /// </summary>
            /// <value>
            ///  The type of the command.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public Type CommandType { get; set; }
        }
    }
}