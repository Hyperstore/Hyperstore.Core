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
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Metadata;

#endregion

namespace Hyperstore.Modeling.Commands
{
    /// <summary>
    ///     Processeur de commande - Execute une commande en créant un pipeline avec les intercepteurs associés
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CommandProcessor<T> : ICommandProcessor<T> where T : IDomainCommand
    {
        #region Classes of CommandProcessor (1)

        private class DescendantComparer : IComparer<int>
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Compares two int objects to determine their relative ordering.
            /// </summary>
            /// <param name="x">
            ///  Int to be compared.
            /// </param>
            /// <param name="y">
            ///  Int to be compared.
            /// </param>
            /// <returns>
            ///  Negative if 'x' is less than 'y', 0 if they are equal, or positive if it is greater.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public int Compare(int x, int y)
            {
                return y - x;
            }
        }

        /// <summary>
        ///     Pipeline d'execution d'une commande
        /// </summary>
        private class CommandPipeline
        {
            #region Enums of CommandPipeline (3)

            private readonly ExecutionCommandContext<T> _context;
            private readonly ICommandHandler<T> _handler;
            private IEnumerable<ICommandInterceptor> _interceptors;

            #endregion Enums of CommandPipeline (3)

            #region Constructors of CommandPipeline (1)

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="context">
            ///  The context.
            /// </param>
            /// <param name="handler">
            ///  The handler.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public CommandPipeline(ExecutionCommandContext<T> context, ICommandHandler<T> handler)
            {
                DebugContract.Requires(context);

                _context = context;
                _handler = handler;
            }

            #endregion Constructors of CommandPipeline (1)

            #region Methods of CommandPipeline (2)

            private BeforeContinuationStatus PerformBeforeInterceptors()
            {
                foreach (var interceptor in _interceptors)
                {
                    if (Session.Current.CancellationToken.IsCancellationRequested)
                        return BeforeContinuationStatus.Abort;

                    var typedInterceptor = interceptor as ICommandInterceptor<T>;
                    try
                    {
                        var status = typedInterceptor != null ? typedInterceptor.OnBeforeExecution(_context) : ((InterceptorWrapper)interceptor).OnBeforeExecution(_context.CurrentSession, _context.Context, _context.Command);

                        // L'intercepteur peut interrompre le déroulement de la commande
                        if (status != BeforeContinuationStatus.Continue)
                            return status;
                    }
                    catch (Exception ex)
                    {
                        _context.Log(new DiagnosticMessage(MessageType.Error, String.Format(ExceptionMessages.Diagnostic_ErrorWhenProcessingBeforeInterceptorforCommandFormat, interceptor.GetType().FullName, _context.Command), "Commands", false, null, ex));
                        return BeforeContinuationStatus.Abort;
                    }
                }
                return BeforeContinuationStatus.Continue;
            }

            private ContinuationStatus PerformAfterInterceptors()
            {
                foreach (var interceptor in _interceptors.Reverse())
                {
                    if (Session.Current.CancellationToken.IsCancellationRequested)
                        return ContinuationStatus.Abort;
                    try
                    {

                        var typedInterceptor = interceptor as ICommandInterceptor<T>;
                        if ((typedInterceptor != null ? typedInterceptor.OnAfterExecution(_context) : ((InterceptorWrapper)interceptor).OnAfterExecution(_context.CurrentSession, _context.Context, _context.Command)) == ContinuationStatus.Abort)
                            return ContinuationStatus.Abort;
                    }
                    catch (Exception ex)
                    {
                        _context.Log(new DiagnosticMessage(MessageType.Error, String.Format(ExceptionMessages.Diagnostic_ErrorWhenProcessingAfterInterceptorforCommandFormat, interceptor.GetType().FullName, _context.Command), "Commands", false, null, ex));
                        return ContinuationStatus.Abort;
                    }
                }
                return ContinuationStatus.Continue;
            }

            private ErrorContinuationStatus PerformOnErrorsInterceptors(Exception ex)
            {
                var abort = false;
                foreach (var interceptor in _interceptors)
                {
                    try
                    {
                        var typedInterceptor = interceptor as ICommandInterceptor<T>;
                        var status = typedInterceptor != null ? typedInterceptor.OnError(_context, ex) : ((InterceptorWrapper)interceptor).OnError(_context.CurrentSession, _context.Context, _context.Command, ex);

                        if (status == ErrorContinuationStatus.Abort)
                            abort = true;
                        else if (status == ErrorContinuationStatus.Retry)
                            return ErrorContinuationStatus.Retry;
                    }
                    catch (Exception ex2)
                    {
                        abort = true;
                        _context.Log(new DiagnosticMessage(MessageType.Error, String.Format(ExceptionMessages.Diagnostic_ErrorWhenProcessingErrorInterceptorforCommandFormat, interceptor.GetType().FullName, _context.Command), "Commands", false, null, ex2));
                    }
                }

                return abort ? ErrorContinuationStatus.Abort : ErrorContinuationStatus.Continue;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Execution d'une commande - Les intercepteurs ne sont pas executés en mode undo/redo.
            /// </summary>
            /// <returns>
            ///  The ContinuationStatus.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public ContinuationStatus Execute()
            {
                var retry = true;
                var firstTry = true;
                var inUndoRedoMode = (Session.Current.Mode & SessionMode.UndoOrRedo) != 0;
                var maxTry = 20;

                while (retry)
                {
                    retry = false;

                    try
                    {
                        // Execution des intercepteurs avant l'exécution de la commande
                        if (_interceptors != null && !inUndoRedoMode && firstTry)
                        {
                            switch (PerformBeforeInterceptors())
                            {
                                case BeforeContinuationStatus.Abort:
                                    return ContinuationStatus.Abort;

                                case BeforeContinuationStatus.SkipCommand:
                                    return ContinuationStatus.Continue;

                                case BeforeContinuationStatus.Continue:
                                default:
                                    break;
                            }
                        }
                        firstTry = false;

                        if (Session.Current.CancellationToken.IsCancellationRequested)
                            return ContinuationStatus.Abort;

                        // Execution de la commade
                        var @event = ExecuteCommand();

                        if (Session.Current.CancellationToken.IsCancellationRequested)
                            return ContinuationStatus.Abort;

                        // Sauvegarde de l'événement généré par la commande
                        if (@event != null)
                        {
                            _context.AddEvent(@event);
                        }

                        // Execution des intercepteurs aprés l'exécution
                        if (_interceptors != null && !inUndoRedoMode)
                        {
                            if (PerformAfterInterceptors() == ContinuationStatus.Abort)
                                return ContinuationStatus.Abort;
                        }
                    }
                    catch (Exception ex)
                    {
                        _context.Log(new DiagnosticMessage(MessageType.Error, String.Format(ExceptionMessages.Diagnostic_ErrorProcessingCommandFormat, _context.Command), "Commands", false, null, ex));

                        if (_interceptors != null && _interceptors.Any())
                        {
                            switch (PerformOnErrorsInterceptors(ex))
                            {
                                case ErrorContinuationStatus.Abort:
                                    return ContinuationStatus.Abort;
                                case ErrorContinuationStatus.Retry:
                                    if (maxTry-- > 0)
                                    {
                                        retry = true;
                                    }
                                    else
                                    {
                                        return ContinuationStatus.Abort;
                                    }
                                    break;
                                case ErrorContinuationStatus.Continue:
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            return ContinuationStatus.Abort;
                        }
                    }
                }

                return ContinuationStatus.Continue;
            }

            private IEvent ExecuteCommand()
            {
                var @event = this._handler != null ? this._handler.Handle(this._context) : this._context.ExecuteCommand();
                return @event;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Registers this instance.
            /// </summary>
            /// <param name="interceptors">
            ///  The interceptors.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void Register(IEnumerable<ICommandInterceptor> interceptors)
            {
                DebugContract.Requires(interceptors);
                _interceptors = interceptors;
            }

            #endregion Methods of CommandPipeline (2)
        }

        #endregion Classes of CommandProcessor (1)

        #region Enums of CommandProcessor (3)

        private ICommandHandler<T> _handler;
        private Dictionary<Type, SortedDictionary<int, List<ICommandInterceptor>>> _interceptors;

        #endregion Enums of CommandProcessor (3)

        #region Constructors of CommandProcessor (1)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="CommandProcessor{T}" /> class.
        /// </summary>
        /// <param name="store">
        ///  The store.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CommandProcessor(IHyperstore store)
        {
            DebugContract.Requires(store);
        }

        #endregion Constructors of CommandProcessor (1)

        #region Methods of CommandProcessor (4)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Processes the specified context.
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  The ContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ContinuationStatus Process(ExecutionCommandContext<T> context)
        {
            DebugContract.Requires(context, "context");

            var pipe = new CommandPipeline(context, _handler);

            // Cette info est activée lors du chargement des metadonnées d'un domainModel.
            // Il n'est pas possible d'activer des rules sur les métadonnées.
            var skipRules = (Session.Current.Mode & SessionMode.SkipInterceptors) == SessionMode.SkipInterceptors;

            // Preconditions
            if (_interceptors != null && !skipRules)
                pipe.Register(GetInterceptorsFor(context.CurrentSession, context.Command).ToList());

            return pipe.Execute();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the rule.
        /// </summary>
        /// <param name="rule">
        ///  The rule.
        /// </param>
        /// <param name="priority">
        ///  The priority.
        /// </param>
        /// ### <exception cref="System.InvalidCastException">
        ///  .
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        public void RegisterInterceptor(ICommandInterceptor rule, int priority)
        {
            Contract.Requires(rule != null, "rule");

            if (_interceptors == null)
                _interceptors = new Dictionary<Type, SortedDictionary<int, List<ICommandInterceptor>>>();

            SortedDictionary<int, List<ICommandInterceptor>> rules;
            if (!_interceptors.TryGetValue(typeof(T), out rules))
            {
                rules = new SortedDictionary<int, List<ICommandInterceptor>>(new DescendantComparer());
                _interceptors.Add(typeof(T), rules);
            }

            List<ICommandInterceptor> list;
            if (!rules.TryGetValue(priority, out list))
            {
                list = new List<ICommandInterceptor>();
                rules.Add(priority, list);
            }

            list.Add(rule);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the handler.
        /// </summary>
        /// <param name="handler">
        ///  The rule.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetHandler(ICommandHandler handler)
        {
            DebugContract.Requires(handler);
            _handler = (ICommandHandler<T>)handler;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the before rules for.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the interceptors fors in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        private IEnumerable<ICommandInterceptor> GetInterceptorsFor(ISession session, T command)
        {
            DebugContract.Requires(command!=null);

            if (command.DomainModel.Name != PrimitivesSchema.DomainModelName)
            {
                SortedDictionary<int, List<ICommandInterceptor>> rules;
                if (_interceptors.TryGetValue(command.GetType(), out rules))
                {
                    foreach (var item in rules)
                    {
                        foreach (var interceptor in item.Value)
                        {
                            var typedInterceptor = interceptor as ICommandInterceptor<T>;
                            if ((typedInterceptor != null ? typedInterceptor.IsApplicableOn(session, command) : ((InterceptorWrapper)interceptor).IsApplicableOn(session, command)))
                                yield return interceptor;
                        }
                    }
                }
            }
        }

        #endregion Methods of CommandProcessor (4)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Clears the interceptors.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void ClearInterceptors()
        {
            if (_interceptors != null)
                _interceptors.Clear();
        }
    }
}