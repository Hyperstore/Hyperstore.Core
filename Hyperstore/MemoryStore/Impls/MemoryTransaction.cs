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

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    internal sealed class MemoryTransaction : ITransaction, ISessionEnlistmentNotification
    {
        private readonly Stack<TransactionStatus> _nestedStatus;
        private readonly SessionIsolationLevel _isolationLevel;
        private int _nextCommandId;
        private readonly ITransactionManager _transactionManager;
        private bool _aborted;
        private TransactionStatus _currentStatus;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an increment current command identifier.
        /// </summary>
        /// <returns>
        ///  an increment current command identifier.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int GetAnIncrementCurrentCommandId()
        {
            return _nextCommandId++;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the status.
        /// </summary>
        /// <value>
        ///  The status.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public TransactionStatus Status
        {
            get { return _currentStatus; }
        }

        internal MemoryTransaction(IHyperstoreTrace trace, ITransactionManager transactionManager, long id, SessionIsolationLevel isolationLevel)
        {
            DebugContract.Requires(transactionManager);
            DebugContract.Requires(trace);

            if (isolationLevel != SessionIsolationLevel.ReadCommitted && isolationLevel != SessionIsolationLevel.Serializable)
                throw new ArgumentOutOfRangeException("Only ReadCommitted or Serializable is allowed.");

            Id = id;
            _isolationLevel = isolationLevel;
            _transactionManager = transactionManager;
            _nestedStatus = new Stack<TransactionStatus>();
            PushNestedTransaction();
            _currentStatus = TransactionStatus.Active;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Pushes the nested transaction.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void PushNestedTransaction()
        {
            _nestedStatus.Push(TransactionStatus.Active);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long Id { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session isolation level.
        /// </summary>
        /// <value>
        ///  The session isolation level.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public SessionIsolationLevel SessionIsolationLevel
        {
            get { return _isolationLevel; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is nested.
        /// </summary>
        /// <value>
        ///  true if this instance is nested, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsNested
        {
            get { return _nestedStatus.Count > 1; }
        }

        bool ISessionEnlistmentNotification.NotifyPrepare()
        {
            return !_aborted ;
        }

        void ISessionEnlistmentNotification.NotifyCommit()
        {
          //  lock (this)
            {
                this._currentStatus = TransactionStatus.Committed;
                _transactionManager.OnTransactionTerminated(this);
            }
        }

        void ISessionEnlistmentNotification.NotifyRollback()
        {
            this._currentStatus = TransactionStatus.Aborted;
          //  lock (this)
            {
                _transactionManager.OnTransactionTerminated(this);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Commits this instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///  Thrown when a supplied object has been disposed.
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        public void Commit()
        {
            if (_nestedStatus.Count == 0)
                throw new ObjectDisposedException("MemoryTransaction");

            // Pop / Push car TransactionStatus est une valeur
            var current = _nestedStatus.Pop();
            if (current == TransactionStatus.Active)
                current = TransactionStatus.Committed;
            _nestedStatus.Push(current);
        }

        #region IDisposable Members

        /////-------------------------------------------------------------------------------------------------
        ///// <summary>
        /////  Finaliser.
        ///// </summary>
        /////-------------------------------------------------------------------------------------------------
        //~MemoryTransaction()
        //{
        //    Dispose(false);
        //}

        private void Dispose(bool disposing)
        {
            var current = _nestedStatus.Pop();
            if (current != TransactionStatus.Committed)
                _aborted = true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Est ce que la transaction en cours est la transaction active (ou une de ces sous-transactions)
        /// </summary>
        /// <param name="xid">
        ///  ID de la transaction à tester.
        /// </param>
        /// <returns>
        ///  True si elle fait partie de la transaction en cours.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsInTransactionScope(long xid)
        {
            return xid == Id;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the active transactions when started.
        /// </summary>
        /// <value>
        ///  The active transactions when started.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<MemoryTransaction> ActiveTransactionsWhenStarted { set; get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Est ce que la transaction.
        /// </summary>
        /// <param name="xid">
        ///  .
        /// </param>
        /// <returns>
        ///  true if in active transaction when started, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsInActiveTransactionWhenStarted(long xid)
        {
            return ActiveTransactionsWhenStarted != null && ActiveTransactionsWhenStarted.Any(x => x.IsInTransactionScope(xid));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("Transaction id={0}, Mode={1}, SessionIsolationLevel={2}", Id, Status, SessionIsolationLevel);
        }
    }
}