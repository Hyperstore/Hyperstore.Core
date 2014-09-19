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
        /// <param name="readOnly">
        ///  (Optional) true to read only.
        /// </param>
        /// <returns>
        ///  A MemoryTransaction.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        MemoryTransaction BeginTransaction(SessionIsolationLevel mode = SessionIsolationLevel.Unspecified, bool readOnly=false);

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