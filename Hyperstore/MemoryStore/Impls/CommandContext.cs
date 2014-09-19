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
    /// <summary>
    ///     Contexte d'exécution d'une commande sur le store transactionnel
    /// </summary>
    internal sealed class CommandContext : IDisposable
    {
        #region fields

        // Stocke la transaction si elle est a été créée par le contexte (dans le cas ou il n'y avait pas de transaction en cours)
        // Cette transaction aura la même durée de vie que le contexte
        //Transaction courante
        private readonly IEnumerable<MemoryTransaction> _activeTransactionsWhenStarted;
        private readonly MemoryTransaction _currentTransaction;
        private readonly MemoryTransaction _transaction;
        private readonly ITransactionManager _transactionManager;
        // Liste des transactions actives au moment de la création du contexte

        #endregion

        #region Properties

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Current transaction.
        /// </summary>
        /// <value>
        ///  The transaction.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public MemoryTransaction Transaction
        {
            get { return _currentTransaction; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Current command id.
        /// </summary>
        /// <value>
        ///  The identifier of the command.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int CommandId { get; private set; }

        #endregion

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Create a new command context. If there is no current transaction, a transaction is created
        ///  and will be terminated when the context will be disposed.
        /// </summary>
        /// <param name="transactionManager">
        ///  .
        /// </param>
        /// <param name="readOnly">
        ///  true to read only.
        /// </param>
        /// <param name="isolationLevel">
        ///  (Optional)
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CommandContext(ITransactionManager transactionManager, bool readOnly=false, SessionIsolationLevel isolationLevel = SessionIsolationLevel.Unspecified)
        {
            DebugContract.Requires(transactionManager, "transactionManager");

            _transactionManager = transactionManager;
            _currentTransaction = _transactionManager.CurrentTransaction ?? (_transaction = transactionManager.BeginTransaction(isolationLevel, readOnly));

            // On s'assure qu' il existe une transaction courante

            if (_currentTransaction.SessionIsolationLevel != SessionIsolationLevel.Serializable)
                _activeTransactionsWhenStarted = transactionManager.GetActiveTransactions();

            // N° de la commande dans le contexte de la transaction courante
            CommandId = _currentTransaction.GetAnIncrementCurrentCommandId();
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Completes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Complete()
        {
            // Si la transaction est 'tenue' par le contexte, on la committe
            if (_transaction != null)
                _transaction.Commit();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Permet de valider si un slot est valide dans le contexte de la transaction courante.
        /// </summary>
        /// <param name="val">
        ///  Slot à tester.
        /// </param>
        /// <returns>
        ///  true if valid in snapshot, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsValidInSnapshot(ISlot val)
        {
            DebugContract.Requires(val, "val");

            // http://momjian.us/main/writings/pgsql/mvcc.pdf p11
            // ((Xmin == my-transaction && inserted by the current transaction
            //    Cmin < my-command && before this command, and
            //    (Xmax is null || the row has not been deleted, or
            //    (Xmax == my-transaction && it was deleted by the current transaction
            //    Cmax >= my-command))) but not before this command,
            // || or
            //    (Xmin is committed && the row was inserted by a committed transaction, and
            //    (Xmax is null || the row has not been deleted, or
            //    (Xmax == my-transaction && the row is being deleted by this transaction
            //    Cmax >= my-command) || but it’s not deleted "yet", or
            //    (Xmax != my-transaction && the row was deleted by another transaction
            //    Xmax is not committed)))) that has not been committed
            //
            if (_currentTransaction.IsInTransactionScope(val.XMin) && val.CMin < CommandId && (val.XMax == null || (_currentTransaction.IsInTransactionScope(val.XMax.Value) && val.CMin >= CommandId)))
                return true;

            if (IsTransactionValid(val.XMin))
            {
                if (val.XMax == null || (_currentTransaction.IsInTransactionScope(val.XMax.Value) && val.CMin >= CommandId))
                    return true;

                if (!_currentTransaction.IsInTransactionScope(val.XMax.Value))
                {
                    if (!IsTransactionValid(val.XMax.Value))
                        return true;
                }
            }
            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  http://www.postgresql.org/files/developer/transactions.pdf (p16)
        /// </summary>
        /// <param name="xid">
        ///  The xid.
        /// </param>
        /// <returns>
        ///  true if transaction valid, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool IsTransactionValid(long xid)
        {
            // http://www.postgresql.org/files/developer/transactions.pdf
            // "Snapshots" filter away active transactions
            //
            // If Transaction A commits while Transaction B is running, we don’t want B to
            // suddenly start seeing A’s updates partway through.
            //  • Hence, we make a list at transaction start of which transactions are currently being
            //    run by other backends.
            //    (Cheap shared-memory communication is essential here: we just look in a
            //    shared-memory table, in which each backend records its current transaction number.)
            //  • These transaction IDs will never be considered valid by the current transaction,
            //    even if they are shown to be committed in pg_log or on-row status bits.
            //  • Nor will a transaction with ID higher than the current transaction’s
            //    ever be considered valid.
            //  • These rules ensure that no transaction committing after the current transaction’s
            //    start will be considered committed.
            //  •  Validity is in the eye of the beholder.
            //
            if (_currentTransaction.IsInTransactionScope(xid))
                return true;

            if (Transaction.SessionIsolationLevel == SessionIsolationLevel.Serializable)
            {
                if (xid > Transaction.Id)
                    return false; // Pas valide

                if (IsInActiveTransactionWhenStarted(xid))
                    return false;
            }

            var tx = _transactionManager.GetTransaction(xid);
            return tx == null || tx.Status != TransactionStatus.Aborted;
        }

        // Vérifie si une transaction ne fait pas partie des transactions actives au moment du démarrage de la commande
        private bool IsInActiveTransactionWhenStarted(long xid)
        {
            if (_activeTransactionsWhenStarted != null)
                return _activeTransactionsWhenStarted.Any(x => x.IsInTransactionScope(xid));

            return _currentTransaction.IsInActiveTransactionWhenStarted(xid);
        }

        private void Dispose(bool disposing)
        {
            if (_transaction != null)
                _transaction.Dispose();
        }
    }
}