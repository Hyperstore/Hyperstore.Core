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
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling
{
    internal class RelayBeforeRule<TCommand> : AbstractCommandInterceptor<TCommand> where TCommand : IDomainCommand
    {
        private readonly Func<TCommand, bool> _filter;
        private readonly string _message;
        private readonly RelayBeforeRuleOptions _options;
        private readonly Func<TCommand, bool> _rule;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="rule">
        ///  The rule.
        /// </param>
        /// <param name="message">
        ///  The message.
        /// </param>
        /// <param name="applicableOn">
        ///  The applicable on.
        /// </param>
        /// <param name="options">
        ///  Options for controlling the operation.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RelayBeforeRule(Func<TCommand, bool> rule, string message, Func<TCommand, bool> applicableOn, RelayBeforeRuleOptions options)
        {
            DebugContract.Requires(rule);
            DebugContract.RequiresNotEmpty(message);
            _message = message;
            _rule = rule;
            _filter = applicableOn;
            _options = options;
        }

        #region IBeforeCommandRule<TCommand> Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the before execution action.
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  The BeforeContinuationStatus.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override BeforeContinuationStatus OnBeforeExecution(ExecutionCommandContext<TCommand> context)
        {
            DebugContract.Requires(context);
            if ((RelayBeforeRuleOptions.IgnoreUndoRedo & _options) == RelayBeforeRuleOptions.IgnoreUndoRedo && context.CurrentSession.Mode != SessionMode.Normal)
                return BeforeContinuationStatus.Continue;

            if (_rule(context.Command))
                return BeforeContinuationStatus.Continue;

            if ((RelayBeforeRuleOptions.IgnoreIfFalse & _options) == RelayBeforeRuleOptions.IgnoreIfFalse)
            {
                context.Log(new DiagnosticMessage(MessageType.Warning, _message, "Rules"));
                return BeforeContinuationStatus.SkipCommand;
            }

            context.Log(new DiagnosticMessage(MessageType.Error, _message, "Rules"));
            return BeforeContinuationStatus.Abort;
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
        public override bool IsApplicableOn(ISession session, TCommand command)
        {
            DebugContract.Requires(command!=null);

            return _filter == null || _filter(command);
        }

        #endregion
    }
}