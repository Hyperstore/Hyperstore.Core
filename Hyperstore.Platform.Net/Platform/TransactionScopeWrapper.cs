using Hyperstore.Modeling;
using Hyperstore.Modeling.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Hyperstore.Platform.Net
{
    internal class TransactionScopeWrapper : ITransactionScope
    {
        private readonly TransactionScope _scope;

        private sealed class EnlistNotificationWrapper : IEnlistmentNotification
        {
            private ISessionEnlistmentNotification _transaction;

            public EnlistNotificationWrapper(ITransaction transaction)
            {
                DebugContract.Requires(transaction);
                _transaction = transaction as ISessionEnlistmentNotification;
                DebugContract.Assert(_transaction != null);
            }

            void IEnlistmentNotification.Commit(Enlistment enlistment)
            {
                _transaction.NotifyCommit();
            }

            void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            {
                _transaction.NotifyRollback();
            }

            void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            {
                if(!_transaction.NotifyPrepare())
                    preparingEnlistment.ForceRollback();
                else
                    preparingEnlistment.Prepared();
            }

            void IEnlistmentNotification.Rollback(Enlistment enlistment)
            {
                _transaction.NotifyRollback();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="isolationLevel">
        ///  The isolation level.
        /// </param>
        /// <param name="timeout">
        ///  The timeout.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public TransactionScopeWrapper(SessionIsolationLevel isolationLevel, TimeSpan timeout)
        {
            var level = isolationLevel == SessionIsolationLevel.ReadCommitted ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable;
            var options = new TransactionOptions
            {
                IsolationLevel = Transaction.Current == null ? level : Transaction.Current.IsolationLevel,
                Timeout = timeout
            };
            _scope = new TransactionScope(TransactionScopeOption.Required, options); //, TransactionScopeAsyncFlowOption.Enabled);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Completes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Complete()
        {
            _scope.Complete();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            _scope.Dispose();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enlists the specified transaction.
        /// </summary>
        /// <param name="transaction">
        ///  The transaction.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Enlist(ITransaction transaction)
        {
            if (Transaction.Current != null)
                Transaction.Current.EnlistVolatile(new EnlistNotificationWrapper( transaction ), EnlistmentOptions.None);
        }
    }
}
