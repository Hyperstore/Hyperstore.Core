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
 
#if WP8

#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

#endregion


namespace System.Collections.Concurrent
{
    internal class ConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /* >>> Code copied from the Array class */

        // We impose limits on maximum array lenght in each dimension to allow efficient 
        // implementation of advanced range check elimination in future.
        // Keep in sync with vm\gcscan.cpp and HashHelpers.MaxPrimeArrayLength.
        internal const int MaxArrayLength = 0X7FEFFFFF;

        /* <<< Code copied from the Array class */

        private class Tables
        {
            internal readonly Node[] m_buckets; // A singly-linked list for each bucket.
            internal readonly object[] m_locks; // A set of locks, each guarding a section of the table.
            internal volatile int[] m_countPerLock; // The number of elements guarded by each lock.

            internal Tables(Node[] buckets, object[] locks, int[] countPerLock)
            {
                m_buckets = buckets;
                m_locks = locks;
                m_countPerLock = countPerLock;
            }
        }

        private volatile Tables m_tables; // Internal tables of the dictionary
        private readonly IEqualityComparer<TKey> m_comparer; // Key equality comparer
        private readonly bool m_growLockArray; // Whether to dynamically increase the size of the striped lock
        private int m_budget; // The maximum number of elements per lock before a resize operation is triggered

        // The default concurrency level is DEFAULT_CONCURRENCY_MULTIPLIER * #CPUs. The higher the
        // DEFAULT_CONCURRENCY_MULTIPLIER, the more concurrent writes can take place without interference
        // and blocking, but also the more expensive operations that require all locks become (e.g. table
        // resizing, ToArray, Count, etc). According to brief benchmarks that we ran, 4 seems like a good
        // compromise.
        private const int DEFAULT_CONCURRENCY_MULTIPLIER = 4;

        // The default capacity, i.e. the initial # of buckets. When choosing this value, we are making
        // a trade-off between the size of a very small dictionary, and the number of resizes when
        // constructing a large dictionary. Also, the capacity should not be divisible by a small prime.
        private const int DEFAULT_CAPACITY = 31;

        // The maximum size of the striped lock that will not be exceeded when locks are automatically
        // added as the dictionary grows. However, the user is allowed to exceed this limit by passing
        // a concurrency level larger than MAX_LOCK_NUMBER into the constructor.
        private const int MAX_LOCK_NUMBER = 1024;

        // Whether TValue is a type that can be written atomically (i.e., with no danger of torn reads)
        private static readonly bool s_isValueWriteAtomic = IsValueWriteAtomic();

        private static bool IsValueWriteAtomic()
        {
            Type valueType = typeof(TValue);

            //
            // Section 12.6.6 of ECMA CLI explains which types can be read and written atomically without
            // the risk of tearing.
            //
            // See http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-335.pdf
            //
            bool isAtomic =
                Hyperstore.Modeling.Utils.ReflectionHelper.IsClass(valueType)
                //(valueType.GetTypeInfo().IsClass)
                || valueType == typeof(Boolean)
                || valueType == typeof(Char)
                || valueType == typeof(Byte)
                || valueType == typeof(SByte)
                || valueType == typeof(Int16)
                || valueType == typeof(UInt16)
                || valueType == typeof(Int32)
                || valueType == typeof(UInt32)
                || valueType == typeof(Single);

            if (!isAtomic && IntPtr.Size == 8)
            {
                isAtomic |= valueType == typeof(Double) || valueType == typeof(Int64);
            }

            return isAtomic;
        }

        public ConcurrentDictionary() : this(EqualityComparer<TKey>.Default) { }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer) : this(DefaultConcurrencyLevel, DEFAULT_CAPACITY, true, comparer) { }

        public ConcurrentDictionary(int capacity, IEqualityComparer<TKey> comparer) : this(DefaultConcurrencyLevel, capacity, true, comparer) { }

        internal ConcurrentDictionary(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<TKey> comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            if (comparer == null) throw new ArgumentNullException("comparer");

            // The capacity should be at least as large as the concurrency level. Otherwise, we would have locks that don't guard
            // any buckets.
            if (capacity < concurrencyLevel)
            {
                capacity = concurrencyLevel;
            }

            object[] locks = new object[concurrencyLevel];
            for (int i = 0; i < locks.Length; i++)
            {
                locks[i] = new object();
            }

            int[] countPerLock = new int[locks.Length];
            Node[] buckets = new Node[capacity];
            m_tables = new Tables(buckets, locks, countPerLock);

            m_comparer = comparer;
            m_growLockArray = growLockArray;
            m_budget = buckets.Length / locks.Length;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!this.TryGetValue(key, out result))
                {
                    throw new KeyNotFoundException();
                }
                return result;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                this.TryAddInternal(key, value, true, true, out tValue);
            }
        }
        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TValue result;
            this.TryAddInternal(key, value, false, true, out result);
            return result;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");
            TValue dummy;
            return TryAddInternal(key, value, false, true, out dummy);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");

            return TryRemoveInternal(key, out value, false, default(TValue));
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
        private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
        {
            while (true)
            {
                Tables tables = m_tables;

                int bucketNo, lockNo;
                GetBucketAndLockNo(m_comparer.GetHashCode(key), out bucketNo, out lockNo, tables.m_buckets.Length, tables.m_locks.Length);

                lock (tables.m_locks[lockNo])
                {
                    // If the table just got resized, we may not be holding the right lock, and must retry.
                    // This should be a rare occurence.
                    if (tables != m_tables)
                    {
                        continue;
                    }

                    Node prev = null;
                    for (Node curr = tables.m_buckets[bucketNo]; curr != null; curr = curr.m_next)
                    {
                        if (m_comparer.Equals(curr.m_key, key))
                        {
                            if (matchValue)
                            {
                                bool valuesMatch = EqualityComparer<TValue>.Default.Equals(oldValue, curr.m_value);
                                if (!valuesMatch)
                                {
                                    value = default(TValue);
                                    return false;
                                }
                            }

                            if (prev == null)
                            {
                                Volatile.Write<Node>(ref tables.m_buckets[bucketNo], curr.m_next);
                            }
                            else
                            {
                                prev.m_next = curr.m_next;
                            }

                            value = curr.m_value;
                            tables.m_countPerLock[lockNo]--;
                            return true;
                        }
                        prev = curr;
                    }
                }

                value = default(TValue);
                return false;
            }
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");

            int bucketNo, lockNoUnused;

            // We must capture the m_buckets field in a local variable. It is set to a new table on each table resize.
            Tables tables = m_tables;

            GetBucketAndLockNo(m_comparer.GetHashCode(key), out bucketNo, out lockNoUnused, tables.m_buckets.Length, tables.m_locks.Length);

            // We can get away w/out a lock here.
            // The Volatile.Read ensures that the load of the fields of 'n' doesn't move before the load from buckets[i].
            Node n = Volatile.Read<Node>(ref tables.m_buckets[bucketNo]);

            while (n != null)
            {
                if (m_comparer.Equals(n.m_key, key))
                {
                    value = n.m_value;
                    return true;
                }
                n = n.m_next;
            }

            value = default(TValue);
            return false;
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
        private bool TryAddInternal(TKey key, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue)
        {
            int hashcode = m_comparer.GetHashCode(key);

            while (true)
            {
                int bucketNo, lockNo;

                Tables tables = m_tables;
                GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables.m_buckets.Length, tables.m_locks.Length);

                bool resizeDesired = false;
                bool lockTaken = false;
                try
                {
                    if (acquireLock)
                        Monitor.Enter(tables.m_locks[lockNo], ref lockTaken);

                    // If the table just got resized, we may not be holding the right lock, and must retry.
                    // This should be a rare occurence.
                    if (tables != m_tables)
                    {
                        continue;
                    }

                    // Try to find this key in the bucket
                    Node prev = null;
                    for (Node node = tables.m_buckets[bucketNo]; node != null; node = node.m_next)
                    {
                        if (m_comparer.Equals(node.m_key, key))
                        {
                            // The key was found in the dictionary. If updates are allowed, update the value for that key.
                            // We need to create a new node for the update, in order to support TValue types that cannot
                            // be written atomically, since lock-free reads may be happening concurrently.
                            if (updateIfExists)
                            {
                                if (s_isValueWriteAtomic)
                                {
                                    node.m_value = value;
                                }
                                else
                                {
                                    Node newNode = new Node(node.m_key, value, hashcode, node.m_next);
                                    if (prev == null)
                                    {
                                        tables.m_buckets[bucketNo] = newNode;
                                    }
                                    else
                                    {
                                        prev.m_next = newNode;
                                    }
                                }
                                resultingValue = value;
                            }
                            else
                            {
                                resultingValue = node.m_value;
                            }
                            return false;
                        }
                        prev = node;
                    }

                    // The key was not found in the bucket. Insert the key-value pair.
                    Volatile.Write<Node>(ref tables.m_buckets[bucketNo], new Node(key, value, hashcode, tables.m_buckets[bucketNo]));
                    checked
                    {
                        tables.m_countPerLock[lockNo]++;
                    }

                    //
                    // If the number of elements guarded by this lock has exceeded the budget, resize the bucket table.
                    // It is also possible that GrowTable will increase the budget but won't resize the bucket table.
                    // That happens if the bucket table is found to be poorly utilized due to a bad hash function.
                    //
                    if (tables.m_countPerLock[lockNo] > m_budget)
                    {
                        resizeDesired = true;
                    }
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(tables.m_locks[lockNo]);
                }

                //
                // The fact that we got here means that we just performed an insertion. If necessary, we will grow the table.
                //
                // Concurrency notes:
                // - Notice that we are not holding any locks at when calling GrowTable. This is necessary to prevent deadlocks.
                // - As a result, it is possible that GrowTable will be called unnecessarily. But, GrowTable will obtain lock 0
                //   and then verify that the table we passed to it as the argument is still the current table.
                //
                if (resizeDesired)
                {
                    GrowTable(tables);
                }

                resultingValue = value;
                return true;
            }
        }

        public System.Collections.Generic.ICollection<TValue> Values
        {
            get { return GetValues(); }
        }

        private void GrowTable(Tables tables)
        {
            int locksAcquired = 0;
            try
            {
                // The thread that first obtains m_locks[0] will be the one doing the resize operation
                AcquireLocks(0, 1, ref locksAcquired);

                // Make sure nobody resized the table while we were waiting for lock 0:
                if (tables != m_tables)
                {
                    // We assume that since the table reference is different, it was already resized (or the budget
                    // was adjusted). If we ever decide to do table shrinking, or replace the table for other reasons,
                    // we will have to revisit this logic.
                    return;
                }

                // Compute the (approx.) total size. Use an Int64 accumulation variable to avoid an overflow.
                long approxCount = 0;
                for (int i = 0; i < tables.m_countPerLock.Length; i++)
                {
                    approxCount += tables.m_countPerLock[i];
                }

                //
                // If the bucket array is too empty, double the budget instead of resizing the table
                //
                if (approxCount < tables.m_buckets.Length / 4)
                {
                    m_budget = 2 * m_budget;
                    if (m_budget < 0)
                    {
                        m_budget = int.MaxValue;
                    }
                    return;
                }


                // Compute the new table size. We find the smallest integer larger than twice the previous table size, and not divisible by
                // 2,3,5 or 7. We can consider a different table-sizing policy in the future.
                int newLength = 0;
                bool maximizeTableSize = false;
                try
                {
                    checked
                    {
                        // Double the size of the buckets table and add one, so that we have an odd integer.
                        newLength = tables.m_buckets.Length * 2 + 1;

                        // Now, we only need to check odd integers, and find the first that is not divisible
                        // by 3, 5 or 7.
                        while (newLength % 3 == 0 || newLength % 5 == 0 || newLength % 7 == 0)
                        {
                            newLength += 2;
                        }

                        if (newLength > MaxArrayLength)
                        {
                            maximizeTableSize = true;
                        }
                    }
                }
                catch (OverflowException)
                {
                    maximizeTableSize = true;
                }

                if (maximizeTableSize)
                {
                    newLength = MaxArrayLength;

                    // We want to make sure that GrowTable will not be called again, since table is at the maximum size.
                    // To achieve that, we set the budget to int.MaxValue.
                    //
                    // (There is one special case that would allow GrowTable() to be called in the future: 
                    // calling Clear() on the ConcurrentDictionary will shrink the table and lower the budget.)
                    m_budget = int.MaxValue;
                }

                // Now acquire all other locks for the table
                AcquireLocks(1, tables.m_locks.Length, ref locksAcquired);

                object[] newLocks = tables.m_locks;

                // Add more locks
                if (m_growLockArray && tables.m_locks.Length < MAX_LOCK_NUMBER)
                {
                    newLocks = new object[tables.m_locks.Length * 2];
                    Array.Copy(tables.m_locks, newLocks, tables.m_locks.Length);

                    for (int i = tables.m_locks.Length; i < newLocks.Length; i++)
                    {
                        newLocks[i] = new object();
                    }
                }

                Node[] newBuckets = new Node[newLength];
                int[] newCountPerLock = new int[newLocks.Length];

                // Copy all data into a new table, creating new nodes for all elements
                for (int i = 0; i < tables.m_buckets.Length; i++)
                {
                    Node current = tables.m_buckets[i];
                    while (current != null)
                    {
                        Node next = current.m_next;
                        int newBucketNo, newLockNo;
                        GetBucketAndLockNo(current.m_hashcode, out newBucketNo, out newLockNo, newBuckets.Length, newLocks.Length);

                        newBuckets[newBucketNo] = new Node(current.m_key, current.m_value, current.m_hashcode, newBuckets[newBucketNo]);

                        checked
                        {
                            newCountPerLock[newLockNo]++;
                        }

                        current = next;
                    }
                }

                // Adjust the budget
                m_budget = Math.Max(1, newBuckets.Length / newLocks.Length);

                // Replace tables with the new versions
                m_tables = new Tables(newBuckets, newLocks, newCountPerLock);
            }
            finally
            {
                // Release all locks that we took earlier
                ReleaseLocks(0, locksAcquired);
            }
        }

        private void GetBucketAndLockNo(
                int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
        {
            bucketNo = (hashcode & 0x7fffffff) % bucketCount;
            lockNo = bucketNo % lockCount;
        }

        private static int DefaultConcurrencyLevel
        {
            get { return DEFAULT_CONCURRENCY_MULTIPLIER * Environment.ProcessorCount; }
        }

        private void AcquireAllLocks(ref int locksAcquired)
        {
            // First, acquire lock 0
            AcquireLocks(0, 1, ref locksAcquired);

            // Now that we have lock 0, the m_locks array will not change (i.e., grow),
            // and so we can safely read m_locks.Length.
            AcquireLocks(1, m_tables.m_locks.Length, ref locksAcquired);
        }

        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
        {
            object[] locks = m_tables.m_locks;

            for (int i = fromInclusive; i < toExclusive; i++)
            {
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(locks[i], ref lockTaken);
                }
                finally
                {
                    if (lockTaken)
                    {
                        locksAcquired++;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread safety")]
        private void ReleaseLocks(int fromInclusive, int toExclusive)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                Monitor.Exit(m_tables.m_locks[i]);
            }
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "ConcurrencyCop just doesn't know about these locks")]
        private ReadOnlyCollection<TValue> GetValues()
        {
            int locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);
                List<TValue> values = new List<TValue>();

                for (int i = 0; i < m_tables.m_buckets.Length; i++)
                {
                    Node current = m_tables.m_buckets[i];
                    while (current != null)
                    {
                        values.Add(current.m_value);
                        current = current.m_next;
                    }
                }

                return new ReadOnlyCollection<TValue>(values);
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        private class Node
        {
            internal TKey m_key;
            internal TValue m_value;
            internal volatile Node m_next;
            internal int m_hashcode;

            internal Node(TKey key, TValue value, int hashcode, Node next)
            {
                m_key = key;
                m_value = value;
                m_next = next;
                m_hashcode = hashcode;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_tables.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = Volatile.Read<ConcurrentDictionary<TKey, TValue>.Node>(ref buckets[i]); node != null; node = node.m_next)
                {
                    yield return new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                }
            }
            yield break;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal class ConcurrentQueue<T>
    {
        private volatile Segment m_head;
        private volatile Segment m_tail;

        private const int SEGMENT_SIZE = 32;

        public ConcurrentQueue()
        {
            m_head = m_tail = new Segment(0, this);
        }

        public bool IsEmpty
        {
            get
            {
                Segment head = m_head;
                if (!head.IsEmpty)
                    //fast route 1:
                    //if current head is not empty, then queue is not empty
                    return false;
                else if (head.Next == null)
                    //fast route 2:
                    //if current head is empty and it's the last segment
                    //then queue is empty
                    return true;
                else
                //slow route:
                //current head is empty and it is NOT the last segment,
                //it means another thread is growing new segment 
                {
                    SpinWait spin = new SpinWait();
                    while (head.IsEmpty)
                    {
                        if (head.Next == null)
                            return true;

                        spin.SpinOnce();
                        head = m_head;
                    }
                    return false;
                }
            }
        }

        public void Enqueue(T item)
        {
            SpinWait spin = new SpinWait();
            while (true)
            {
                Segment tail = m_tail;
                if (tail.TryAppend(item))
                    return;
                spin.SpinOnce();
            }
        }

        public bool TryDequeue(out T result)
        {
            while (!IsEmpty)
            {
                Segment head = m_head;
                if (head.TryRemove(out result))
                    return true;
                //since method IsEmpty spins, we don't need to spin in the while loop
            }
            result = default(T);
            return false;
        }

        private class Segment
        {
            //we define two volatile arrays: m_array and m_state. Note that the accesses to the array items 
            //do not get volatile treatment. But we don't need to worry about loading adjacent elements or 
            //store/load on adjacent elements would suffer reordering. 
            // - Two stores:  these are at risk, but CLRv2 memory model guarantees store-release hence we are safe.
            // - Two loads: because one item from two volatile arrays are accessed, the loads of the array references
            //          are sufficient to prevent reordering of the loads of the elements.
            internal volatile T[] m_array;

            // For each entry in m_array, the corresponding entry in m_state indicates whether this position contains 
            // a valid value. m_state is initially all false. 
            internal volatile VolatileBool[] m_state;

            //pointer to the next segment. null if the current segment is the last segment
            private volatile Segment m_next;

            //We use this zero based index to track how many segments have been created for the queue, and
            //to compute how many active segments are there currently. 
            // * The number of currently active segments is : m_tail.m_index - m_head.m_index + 1;
            // * m_index is incremented with every Segment.Grow operation. We use Int64 type, and we can safely 
            //   assume that it never overflows. To overflow, we need to do 2^63 increments, even at a rate of 4 
            //   billion (2^32) increments per second, it takes 2^31 seconds, which is about 64 years.
            internal readonly long m_index;

            //indices of where the first and last valid values
            // - m_low points to the position of the next element to pop from this segment, range [0, infinity)
            //      m_low >= SEGMENT_SIZE implies the segment is disposable
            // - m_high points to the position of the latest pushed element, range [-1, infinity)
            //      m_high == -1 implies the segment is new and empty
            //      m_high >= SEGMENT_SIZE-1 means this segment is ready to grow. 
            //        and the thread who sets m_high to SEGMENT_SIZE-1 is responsible to grow the segment
            // - Math.Min(m_low, SEGMENT_SIZE) > Math.Min(m_high, SEGMENT_SIZE-1) implies segment is empty
            // - initially m_low =0 and m_high=-1;
            private volatile int m_low;
            private volatile int m_high;

            private volatile ConcurrentQueue<T> m_source;

            internal Segment(long index, ConcurrentQueue<T> source)
            {
                m_array = new T[SEGMENT_SIZE];
                m_state = new VolatileBool[SEGMENT_SIZE]; //all initialized to false
                m_high = -1;
                Contract.Assert(index >= 0);
                m_index = index;
                m_source = source;
            }

            internal Segment Next
            {
                get { return m_next; }
            }

            internal bool IsEmpty
            {
                get { return (Low > High); }
            }

            internal void UnsafeAdd(T value)
            {
                Contract.Assert(m_high < SEGMENT_SIZE - 1);
                m_high++;
                m_array[m_high] = value;
                m_state[m_high].m_value = true;
            }

            internal Segment UnsafeGrow()
            {
                Contract.Assert(m_high >= SEGMENT_SIZE - 1);
                Segment newSegment = new Segment(m_index + 1, m_source); //m_index is Int64, we don't need to worry about overflow
                m_next = newSegment;
                return newSegment;
            }

            internal void Grow()
            {
                //no CAS is needed, since there is no contention (other threads are blocked, busy waiting)
                Segment newSegment = new Segment(m_index + 1, m_source);  //m_index is Int64, we don't need to worry about overflow
                m_next = newSegment;
                Contract.Assert(m_source.m_tail == this);
                m_source.m_tail = m_next;
            }

            internal bool TryAppend(T value)
            {
                //quickly check if m_high is already over the boundary, if so, bail out
                if (m_high >= SEGMENT_SIZE - 1)
                {
                    return false;
                }

                //Now we will use a CAS to increment m_high, and store the result in newhigh.
                //Depending on how many free spots left in this segment and how many threads are doing this Increment
                //at this time, the returning "newhigh" can be 
                // 1) < SEGMENT_SIZE - 1 : we took a spot in this segment, and not the last one, just insert the value
                // 2) == SEGMENT_SIZE - 1 : we took the last spot, insert the value AND grow the segment
                // 3) > SEGMENT_SIZE - 1 : we failed to reserve a spot in this segment, we return false to 
                //    Queue.Enqueue method, telling it to try again in the next segment.

                int newhigh = SEGMENT_SIZE; //initial value set to be over the boundary

                //We need do Interlocked.Increment and value/state update in a finally block to ensure that they run
                //without interuption. This is to prevent anything from happening between them, and another dequeue
                //thread maybe spinning forever to wait for m_state[] to be true;
                try
                { }
                finally
                {
                    newhigh = Interlocked.Increment(ref m_high);
                    if (newhigh <= SEGMENT_SIZE - 1)
                    {
                        m_array[newhigh] = value;
                        m_state[newhigh].m_value = true;
                    }

                    //if this thread takes up the last slot in the segment, then this thread is responsible
                    //to grow a new segment. Calling Grow must be in the finally block too for reliability reason:
                    //if thread abort during Grow, other threads will be left busy spinning forever.
                    if (newhigh == SEGMENT_SIZE - 1)
                    {
                        Grow();
                    }
                }

                //if newhigh <= SEGMENT_SIZE-1, it means the current thread successfully takes up a spot
                return newhigh <= SEGMENT_SIZE - 1;
            }

            internal bool TryRemove(out T result)
            {
                SpinWait spin = new SpinWait();
                int lowLocal = Low, highLocal = High;
                while (lowLocal <= highLocal)
                {
                    //try to update m_low
                    if (Interlocked.CompareExchange(ref m_low, lowLocal + 1, lowLocal) == lowLocal)
                    {
                        //if the specified value is not available (this spot is taken by a push operation,
                        // but the value is not written into yet), then spin
                        SpinWait spinLocal = new SpinWait();
                        while (!m_state[lowLocal].m_value)
                        {
                            spinLocal.SpinOnce();
                        }
                        result = m_array[lowLocal];
                        m_array[lowLocal] = default(T); //release the reference to the object. 

                        //if the current thread sets m_low to SEGMENT_SIZE, which means the current segment becomes
                        //disposable, then this thread is responsible to dispose this segment, and reset m_head 
                        if (lowLocal + 1 >= SEGMENT_SIZE)
                        {
                            //  Invariant: we only dispose the current m_head, not any other segment
                            //  In usual situation, disposing a segment is simply seting m_head to m_head.m_next
                            //  But there is one special case, where m_head and m_tail points to the same and ONLY
                            //segment of the queue: Another thread A is doing Enqueue and finds that it needs to grow,
                            //while the *current* thread is doing *this* Dequeue operation, and finds that it needs to 
                            //dispose the current (and ONLY) segment. Then we need to wait till thread A finishes its 
                            //Grow operation, this is the reason of having the following while loop
                            spinLocal = new SpinWait();
                            while (m_next == null)
                            {
                                spinLocal.SpinOnce();
                            }
                            Contract.Assert(m_source.m_head == this);
                            m_source.m_head = m_next;
                        }
                        return true;
                    }
                    else
                    {
                        //CAS failed due to contention: spin briefly and retry
                        spin.SpinOnce();
                        lowLocal = Low; highLocal = High;
                    }
                }//end of while
                result = default(T);
                return false;
            }

            internal bool TryPeek(out T result)
            {
                result = default(T);
                int lowLocal = Low;
                if (lowLocal > High)
                    return false;
                SpinWait spin = new SpinWait();
                while (!m_state[lowLocal].m_value)
                {
                    spin.SpinOnce();
                }
                result = m_array[lowLocal];
                return true;
            }

            internal int Low
            {
                get
                {
                    return Math.Min(m_low, SEGMENT_SIZE);
                }
            }

            internal int High
            {
                get
                {
                    //if m_high > SEGMENT_SIZE, it means it's out of range, we should return
                    //SEGMENT_SIZE-1 as the logical position
                    return Math.Min(m_high, SEGMENT_SIZE - 1);
                }
            }

        }
    }//end of class Segment

    struct VolatileBool
    {
        public VolatileBool(bool value)
        {
            m_value = value;
        }
        public volatile bool m_value;
    }


}
#endif