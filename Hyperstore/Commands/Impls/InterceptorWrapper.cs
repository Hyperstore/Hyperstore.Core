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
 
using Hyperstore.Modeling.Utils;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Hyperstore.Modeling.Commands
{
    internal class InterceptorWrapper : ICommandInterceptor
    {
        private readonly ICommandInterceptor _interceptor;
        private readonly Type _ruleCommandType;
        private Func<ICommandInterceptor, ISession, ISessionContext, IDomainCommand, ContinuationStatus> _after;
        private Func<ICommandInterceptor, ISession, ISessionContext, IDomainCommand, BeforeContinuationStatus> _before;
        private Func<ICommandInterceptor, ISession, ISessionContext, IDomainCommand, Exception, ErrorContinuationStatus> _error;
        private Func<ICommandInterceptor, IDomainCommand, bool> _isApplicable;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="ruleCommandType">
        ///  Type of the rule command.
        /// </param>
        /// <param name="interceptor">
        ///  The interceptor.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public InterceptorWrapper(Type ruleCommandType, ICommandInterceptor interceptor)
        {
            DebugContract.Requires(ruleCommandType);
            DebugContract.Requires(interceptor);
            _ruleCommandType = ruleCommandType;
            _interceptor = interceptor;
        }

        private LambdaExpression PrepareInvocation(string methodName, bool errorMethod = false)
        {
            // Cast
            var interceptorType = typeof(ICommandInterceptor<>).MakeGenericType(_ruleCommandType);
            var contextType = typeof(ExecutionCommandContext<>).MakeGenericType(_ruleCommandType);

            var pcmd = Expression.Parameter(typeof(IDomainCommand));
            var psession = Expression.Parameter(typeof(ISession));
            var plog = Expression.Parameter(typeof(ISessionContext));

            var prule = Expression.Parameter(typeof(ICommandInterceptor), "ctx");
            var contextCtor = ReflectionHelper.GetProtectedConstructor(contextType, new[] { typeof(ISession), typeof(ISessionContext), _ruleCommandType });
            var methodInfo = ReflectionHelper.GetMethod(interceptorType, methodName).First();

            var context = Expression.New(contextCtor, psession, plog, Expression.Convert(pcmd, _ruleCommandType));
            if (errorMethod == false)
            {
                var call = Expression.Call(Expression.Convert(prule, interceptorType), methodInfo, context);
                return Expression.Lambda(Expression.Block(call), prule, psession, plog, pcmd);
            }

            var pException = Expression.Parameter(typeof(Exception));
            var call2 = Expression.Call(Expression.Convert(prule, interceptorType), methodInfo, context, pException);
            return Expression.Lambda(Expression.Block(call2), prule, psession, plog, pcmd, pException);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the before execution action.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  The BeforeContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public BeforeContinuationStatus OnBeforeExecution(ISession session, ISessionContext context, IDomainCommand command)
        {
            if (_before == null)
            {
                _before = PrepareInvocation("OnBeforeExecution").Compile() as Func<ICommandInterceptor, ISession, ISessionContext, IDomainCommand, BeforeContinuationStatus>;
            }
            return _before(_interceptor, session, context, command);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the after execution action.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  The ContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ContinuationStatus OnAfterExecution(ISession session, ISessionContext context, IDomainCommand command)
        {
            if (_after == null)
            {
                _after = PrepareInvocation("OnAfterExecution").Compile() as Func<ICommandInterceptor, ISession, ISessionContext, IDomainCommand, ContinuationStatus>;
            }
            return _after(_interceptor, session, context, command);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the error action.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <param name="ex">
        ///  The ex.
        /// </param>
        /// <returns>
        ///  The ErrorContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ErrorContinuationStatus OnError(ISession session, ISessionContext context, IDomainCommand command, Exception ex)
        {
            if (_error == null)
            {
                _error = PrepareInvocation("OnError", true).Compile() as Func<ICommandInterceptor, ISession, ISessionContext, IDomainCommand, Exception, ErrorContinuationStatus>;
            }
            return _error(_interceptor, session, context, command, ex);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'command' is applicable on.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="command">
        ///  The command.
        /// </param>
        /// <returns>
        ///  true if applicable on, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsApplicableOn(ISession session, IDomainCommand command)
        {
            if (_isApplicable == null)
            {
                var interceptorType = typeof(ICommandInterceptor<>).MakeGenericType(_ruleCommandType);

                var pcmd = Expression.Parameter(typeof(IDomainCommand));
                var methodInfo = ReflectionHelper.GetMethod(interceptorType, "IsApplicableOn").First();
                var prule = Expression.Parameter(typeof(ICommandInterceptor), "ctx");

                var call = Expression.Call(Expression.Convert(prule, interceptorType), methodInfo, Expression.Constant(session), Expression.Convert(pcmd, _ruleCommandType));
                _isApplicable = Expression.Lambda(call, prule, pcmd).Compile() as Func<ICommandInterceptor, IDomainCommand, bool>;
            }
            return _isApplicable(_interceptor, command);
        }
    }
}
