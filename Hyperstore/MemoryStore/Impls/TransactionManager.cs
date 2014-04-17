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
using System.Threading;
using Hyperstore.Modeling.Ioc;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    internal class TransactionManager : ITransactionManager
    {
        private const string CONTEXT_KEY = "__MTM__";

        private static long _transactionNumber = 1;
        private readonly List<MemoryTransaction> _activeTransactions = new List<MemoryTransaction>(8419);
        private readonly object _sync = new object();
        private readonly IHyperstoreTrace _trace;

        private Dictionary<long, MemoryTransaction> _transactions = new Dictionary<long, MemoryTransaction>(8419);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public TransactionManager(IDependencyResolver resolver)
        {
            DebugContract.Requires(resolver);
            _trace = resolver.Resolve<IHyperstoreTrace>() ?? new EmptyHyperstoreTrace();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Indexer to get items within this collection using array index syntax.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the entry to access.
        /// </param>
        /// <returns>
        ///  The indexed item.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public MemoryTransaction this[long index]
        {
            get { return GetTransaction(index); }
        }
        private long? LowerActiveTransaction
        {
            get
            {
                lock (_activeTransactions)
                {
                    return _activeTransactions.Where(t => t.Status == TransactionStatus.Active)
                            .Min(t => (long?) t.Id);
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the current transaction.
        /// </summary>
        /// <exception cref="NotInTransactionException">
        ///  Thrown when a Not In Transaction error condition occurs.
        /// </exception>
        /// <value>
        ///  The current transaction.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual MemoryTransaction CurrentTransaction
        {
            get
            {
                var session = Session.Current;
                if (session == null)
                    return null;
                    //throw new NotInTransactionException();

                var ctx = session.GetContextInfo<MemoryTransaction>(CONTEXT_KEY);
                return ctx;
            }
            set
            {
                var session = Session.Current;
                if (session == null)
                    throw new NotInTransactionException();

                session.SetContextInfo(CONTEXT_KEY, value);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event raises when a transaction is completed (committed or aborted)
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler<TransactionCompletedEventArgs> TransactionCompleted;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Start a new transaction.
        /// </summary>
        /// <param name="isolationLevel">
        ///  .
        /// </param>
        /// <returns>
        ///  A MemoryTransaction.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public MemoryTransaction BeginTransaction(SessionIsolationLevel isolationLevel)
        {
            if (isolationLevel == SessionIsolationLevel.Unspecified)
            {
                var s = Session.Current;
                isolationLevel = s != null ? Session.Current.SessionIsolationLevel : SessionIsolationLevel.ReadCommitted;
            }

            var tx = CreateTransaction(isolationLevel);

            if (isolationLevel == SessionIsolationLevel.Serializable)
            {
                var list = GetActiveTransactions();
                tx.ActiveTransactionsWhenStarted = list;
                _trace.WriteTrace(TraceCategory.MemoryStore, "Active transaction when tx {0} starts : {1}", tx.Id, list != null
                        ? String.Join(",", list.Select(t => t.Id)
                                .ToArray())
                        : "");
            }

            return tx;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the transaction terminated action.
        /// </summary>
        /// <param name="transaction">
        ///  The transaction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void OnTransactionTerminated(MemoryTransaction transaction)
        {
            Debug.Assert(transaction.Status != TransactionStatus.Active);
            Debug.Assert(!transaction.IsNested);

            lock (_activeTransactions)
                _activeTransactions.Remove(transaction);

            OnTransactionEnded(transaction, transaction.Status == TransactionStatus.Committed);
            _trace.WriteTrace(TraceCategory.MemoryStore, "Memory transaction ended " + transaction.Id);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the current active transactions.
        /// </summary>
        /// <returns>
        ///  The active transactions.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<MemoryTransaction> GetActiveTransactions()
        {
            lock (_activeTransactions)
            {
                return _activeTransactions.Count == 0 ? null : new List<MemoryTransaction>(_activeTransactions);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Suppression des transactions committées 
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Vacuum()
        {
            try
            {
                // http://www.packtpub.com/article/transaction-model-of-postgresql
                // How do you identify Slots that are expired? 
                //   Xmax will be a transaction that has long ago committed( committed before any running transaction).
                var lowerActiveTransaction = LowerActiveTransaction;

                var tmp = new Dictionary<long, MemoryTransaction>(_transactions.Count);

                lock (_sync)
                {
                    foreach (var pair in _transactions)
                    {
                        var toBeDeleted = false;
                        var tx = pair.Value;
                        if (tx.Status == TransactionStatus.Committed && (lowerActiveTransaction == null || tx.Id < lowerActiveTransaction))
                            toBeDeleted = true;

                        if (!toBeDeleted)
                            tmp.Add(pair.Key, pair.Value);
                    }

                    _transactions = tmp;
                }
            }
            catch
            {
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get a transaction by id.
        /// </summary>
        /// <param name="index">
        ///  Zero-based index of the.
        /// </param>
        /// <returns>
        ///  The transaction.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public MemoryTransaction GetTransaction(long index)
        {
            if (index == 0)
                return null;
            MemoryTransaction tx = null;
            _transactions.TryGetValue(index, out tx);
            return tx;
        }

        /// <summary>
        ///     Création d'une transaction (Scope = RequiredNested)
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        private MemoryTransaction CreateTransaction(SessionIsolationLevel isolationLevel)
        {
            var current = CurrentTransaction;
            if (current == null)
            {
                var xid = Interlocked.Increment(ref _transactionNumber);
                _trace.WriteTrace(TraceCategory.MemoryStore, "Create memory transaction {0} - Thread {1}", xid, ThreadHelper.CurrentThreadId);
                current = new MemoryTransaction(_trace, this, xid, isolationLevel);
                if (Session.Current != null)
                    CurrentTransaction = current;
                
                lock (_sync)
                {
                    _transactions.Add(current.Id, current);
                    lock (_activeTransactions)
                    {
                        _activeTransactions.Add(current);
                    }

                    if (Session.Current != null)
                        Session.Current.Enlist(current);
                }
            }
            else
                current.PushNestedTransaction();

            return current;
        }

        private void OnTransactionEnded(MemoryTransaction transaction, bool committed)
        {
            DebugContract.Requires(transaction);

            var tmp = TransactionCompleted;
            if (tmp != null)
            {
                try
                {
                    tmp(transaction, new TransactionCompletedEventArgs(transaction.Id, committed, transaction.IsNested));
                }
                catch
                {
                }
            }
        }
    }
}