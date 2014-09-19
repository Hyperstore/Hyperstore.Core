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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hyperstore.Modeling.Container;
using Hyperstore.Modeling.MemoryStore;

#endregion

namespace Hyperstore.Modeling
{
    /// <summary>
    ///     Lock manager
    /// </summary>
    internal class LockManager : ILockManager, IDisposable
    {
        private const int DEFAULT_DEADLOCK_TIME = 60000;
        private readonly Dictionary<object, LockInfo> _locks = new Dictionary<object, LockInfo>();
        private readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The dead lock limit time in milliseconds.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public int DeadLockLimitTimeInMs;
        private readonly IHyperstoreTrace _trace;

        internal LockManager(IServicesContainer services)
        {
            Contract.Requires(services, "services");

            _trace = services.Resolve<IHyperstoreTrace>() ?? new EmptyHyperstoreTrace();

            var defaultMaxTimeBeforeDeadlockInSeconds = services.GetSettingValue<int?>(Setting.MaxTimeBeforeDeadlockInMs);
            var n = defaultMaxTimeBeforeDeadlockInSeconds != null ? defaultMaxTimeBeforeDeadlockInSeconds.Value : DEFAULT_DEADLOCK_TIME;
            if (n < 0)
                n = DEFAULT_DEADLOCK_TIME;
            DeadLockLimitTimeInMs = n;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
        }

        #region Lock

        private struct LockWrapper : IDisposable
        {
            private LockInfo _lockInfo;
            private LockManager _lockManager;

            internal LockWrapper(LockManager lockManager = null, LockInfo lockInfo = null)
            {
                _lockInfo = lockInfo;
                _lockManager = lockManager;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
            ///  resources.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Dispose()
            {
                if (_lockInfo != null)
                    _lockManager.ReleaseLock(_lockInfo);
                _lockInfo = null;
                _lockManager = null;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Acquires the lock.
        /// </summary>
        /// <exception cref="DeadLockException">
        ///  Thrown when a Dead Lock error condition occurs.
        /// </exception>
        /// <exception cref="SerializableTransactionException">
        ///  Thrown when a Serializable Transaction error condition occurs.
        /// </exception>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDisposable AcquireLock(ISession session, object key, LockType mode)
        {
            Contract.Requires(key, "key");
            Contract.Requires(session, "session");

            //if (session.IsDisposing)
            //{
            //    throw new Exception("Impossible d'acquerir un lock sur une session en train de se terminer. Si vous avez lancé des traitements en parralléles pendant la session, assurez vous qu'ils soient terminé quand la session est disposé.");
            //}

            LockInfo info;
            var deadlockTimeLimit = DeadLockLimitTimeInMs;

            // Un lock est tenu par la transaction englobante.
            // Il sera libéré quand celle ci se terminera
            var txId = session.SessionId;

            var resource = key.ToString();
            _trace.WriteTrace(TraceCategory.LockManager, "Try to acquire lock for {0} in tx {1}", resource, txId);

            // On va essayer d'obtenir un lock virtuel pour la ressource demandée
            // Un lock virtuel est une structure qui contient le lock physique et le map entre la transaction qui a obtenu le lock et la ressource concernée
            _sync.EnterWriteLock();
            try
            {
                // Etat du lock virtuel avant d'essayer d'avoir le lock physique                    
                if (!_locks.TryGetValue(resource, out info))
                {
                    // Ce lock n'a jamais été traité, on le crée
                    info = new LockInfo
                           {
                               SessionId = txId,
                               Ressource = resource,
                               Lock = new ReaderWriterLockSlim(),
                               SessionStatus = TransactionStatus.Active,
                               Mode = mode
                           };
                    // Nouveau lock, on peut obtenir tout de suite le lock physique
                    if ((mode & LockType.Exclusive) == LockType.Exclusive)
                        info.Lock.EnterWriteLock();
                    else
                        info.Lock.EnterUpgradeableReadLock();

                    _locks[resource] = info;
                    session.Locks.Add(info);
                    info.AddRef();

                    _trace.WriteTrace(TraceCategory.LockManager, "Lock acquired for {0} in tx {1}", resource, txId);

                    return new LockWrapper(this, info);
                }

                // Ce lock existe dèjà.
                // Pas grave si c'est dans le même thread
                if (session.SessionId == info.SessionId)
                {
                    if (info.Mode != mode)
                    {
                        if (info.Mode == LockType.Shared)
                        {
                            info.Lock.EnterWriteLock();
                            info.Mode = mode;
                            info.Promoted = true;
                        }
                    }

                    _trace.WriteTrace(TraceCategory.LockManager, "Use lock from parent transaction for {0} in tx {1}", resource, txId);
                    return new LockWrapper(); // Il n'est pas possible de libèrer ce lock ici
                }

                info.AddRef();
                _trace.WriteTrace(TraceCategory.LockManager, "Waiting lock for {0} in tx {1}", resource, txId);
            }
            finally
            {
                _sync.ExitWriteLock();
            }

            // Le lock est dèjà pris.
            // On va essayer de l'obtenir mais avec un time out (pour pouvoir traiter le dead lock)
            if (((LockType.Exclusive & mode) == LockType.Exclusive ? !info.Lock.TryEnterWriteLock(deadlockTimeLimit) : !info.Lock.TryEnterUpgradeableReadLock(deadlockTimeLimit)))
            {
                _trace.WriteTrace(TraceCategory.LockManager, "Deadlock for {0} in tx {1}", resource, txId);
                if (info.DecRef() == 0)
                {
                    _sync.EnterWriteLock();
                    try
                    {
                        if (info.DecRef() == 0)
                            _locks.Remove(resource);
                    }
                    finally
                    {
                        _sync.ExitWriteLock();
                    }
                }

                // On ne l'a pas
                throw new DeadLockException(); // c'est un dead lock, on abandonne
            }

            _trace.WriteTrace(TraceCategory.LockManager, "Transaction {0} has released lock for {1}", info.SessionId, resource);

            // On vient d'avoir le lock 
            // Si on est pas dans le scope de la transaction qui tenait le lock et que le niveau de sérialisation est Serialize alors
            //  Si la transaction qui tenait le lock s'est correctement terminé  il y a conflit
            if (session.SessionIsolationLevel == SessionIsolationLevel.Serializable && session.SessionId != info.SessionId)
            {
                // Est ce que la transaction concurrente s'est bien terminée ?            
                // Si concurrentTransaction est null, c'est que la transaction référencée s'est bien terminée et qu'elle a été purgé par le vaccum
                if (info.SessionStatus == TransactionStatus.Committed)
                {
                    // Si on est en mode exclusive, on va générer une erreur
                    if ((mode & LockType.Exclusive) == LockType.Exclusive)
                    {
                        if ((mode & LockType.ExclusiveWait) != LockType.ExclusiveWait)
                        {
                            _trace.WriteTrace(TraceCategory.LockManager, "Serialize transaction error for {0}", resource);

                            _sync.EnterWriteLock();
                            try
                            {
                                if (info.DecRef() == 0)
                                    _locks.Remove(resource);
                            }
                            finally
                            {
                                _sync.ExitWriteLock();
                            }

                            info.Lock.ExitWriteLock();

                            throw new SerializableTransactionException(resource);
                        }
                    }
                }
            }

            // Sinon tout est OK
            _trace.WriteTrace(TraceCategory.LockManager, "Lock acquired (after waiting) for {0} in tx {1}", resource, txId);
            info.SessionId = txId;
            info.SessionStatus = TransactionStatus.Active;
            info.Mode = mode;
            _sync.EnterWriteLock();
            try
            {
                session.Locks.Add(info);
            }
            finally
            {
                _sync.ExitWriteLock();
            }

            return new LockWrapper(this, info);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the locks.
        /// </summary>
        /// <param name="locks">
        ///  The locks.
        /// </param>
        /// <param name="sessionAborted">
        ///  true if session aborted.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void ReleaseLocks(ICollection<ILockInfo> locks, bool sessionAborted)
        {
            DebugContract.Requires(locks);
            foreach (LockInfo lockInfo in locks)
            {
                lockInfo.SessionStatus = sessionAborted ? TransactionStatus.Aborted : TransactionStatus.Committed;
                ReleaseLock(lockInfo);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the specified ressource has lock.
        /// </summary>
        /// <param name="ressource">
        ///  The ressource.
        /// </param>
        /// <returns>
        ///  A LockType.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public LockType HasLock(object ressource)
        {
            Contract.Requires(ressource, "ressource");
            _sync.EnterReadLock();
            try
            {
                LockInfo info;
                if (_locks.TryGetValue(ressource, out info))
                    return info.Mode;
                return LockType.None;
            }
            finally
            {
                _sync.ExitReadLock();
            }
        }

        private void ReleaseLock(LockInfo lockInfo)
        {
            DebugContract.Requires(lockInfo, "lockInfo");

            if (lockInfo.Lock.IsWriteLockHeld)
                lockInfo.Lock.ExitWriteLock();

            if (lockInfo.Lock.IsUpgradeableReadLockHeld)
                lockInfo.Lock.ExitUpgradeableReadLock();

            _trace.WriteTrace(TraceCategory.LockManager, "Release lock for {0} in tx {1}", lockInfo.Ressource, lockInfo.SessionId);

            // Suppression des locks associés à cette transaction si il n'y en a pas en attente.
            if (lockInfo.DecRef() == 0)
            {
                Debug.Assert(lockInfo.Lock.WaitingWriteCount == 0 && lockInfo.Lock.WaitingUpgradeCount == 0);
                _sync.EnterWriteLock();
                try
                {
                    _locks.Remove(lockInfo.Ressource);
                }
                finally
                {
                    _sync.ExitWriteLock();
                }
            }
        }

        #endregion

#if TEST
        internal bool IsEmpty()
        {
            return _locks.Count == 0;
        }
#endif
    }
}