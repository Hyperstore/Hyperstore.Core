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

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    /// <summary>
    ///     Transaction manager.
    ///     Gestion des transacions dans le cadre du store transactionnel
    /// </summary>
    internal interface ITransactionManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the current transaction.
        /// </summary>
        /// <value>
        ///  The current transaction.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        MemoryTransaction CurrentTransaction { get; }
        /// <summary>
        ///     Event raises when a transaction is completed (committed or aborted)
        /// </summary>
        event EventHandler<TransactionCompletedEventArgs> TransactionCompleted;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Start a new transaction.
        /// </summary>
        /// <param name="mode">
        ///  (Optional)
        /// </param>
        /// <returns>
        ///  A MemoryTransaction.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        MemoryTransaction BeginTransaction(SessionIsolationLevel mode = SessionIsolationLevel.Unspecified);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get a transaction by id.
        /// </summary>
        /// <param name="tid">
        ///  Transaction id.
        /// </param>
        /// <returns>
        ///  The transaction.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        MemoryTransaction GetTransaction(long tid);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get the current active transactions.
        /// </summary>
        /// <returns>
        ///  The active transactions.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<MemoryTransaction> GetActiveTransactions();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Vacuums this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Vacuum();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the transaction terminated action.
        /// </summary>
        /// <param name="transaction">
        ///  The transaction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void OnTransactionTerminated(MemoryTransaction transaction);
    }
}