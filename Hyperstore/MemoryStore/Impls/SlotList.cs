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

using Hyperstore.Modeling.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    /// <summary>
    ///     Liste de tous les slots liés à une valeur
    /// </summary>
    /// <remarks>
    ///     Cette liste est implémentée sous la forme d'une liste chainée afin de pouvoir itérer dessus alors qu'elle est
    ///     modifiée (TODO voir si cela à un sens maintenant)
    /// </remarks>
    internal sealed class SlotList :  ISlotList
    {
        class SlotEntry
        {
            public SlotEntry Next;
            public ISlot Slot;
        }

        private readonly NodeType _elementType;
        private readonly object _ownerKey;
        private SlotEntry _head;
        private int _hits;
        private long _lastAccess;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="elementType">
        ///  The type of the element.
        /// </param>
        /// <param name="ownerKey">
        ///  (Optional) The owner key.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SlotList(NodeType elementType, object ownerKey = null)
        {
            DebugContract.Requires(elementType != NodeType.Property || ownerKey != null);

            _ownerKey = ownerKey;
            _elementType = elementType;
            _head = new SlotEntry();
            Mark();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<ISlot> GetSlots()
        {
            SlotEntry entry = _head.Next;
            while (entry != null)
            {
                yield return entry.Slot;
                entry = entry.Next;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the key that owns this item.
        /// </summary>
        /// <value>
        ///  The owner key.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object OwnerKey
        {
            get { return _ownerKey; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the type of the element.
        /// </summary>
        /// <value>
        ///  The type of the element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public NodeType ElementType
        {
            get { return _elementType; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Nombre de fois ou cette valeur a été accédée.
        /// </summary>
        /// <value>
        ///  The hits.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Hits
        {
            get { return _hits; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Date (en nombre de ticks) du dernier accés.
        /// </summary>
        /// <value>
        ///  The last access.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long LastAccess
        {
            get { return _lastAccess; }
        }

        internal void Add(ISlot slot)
        {
            DebugContract.Requires(slot);
            SlotEntry entry = new SlotEntry();
            entry.Slot = slot;
            do
            {
                entry.Next = _head.Next;
            }
            while (Interlocked.CompareExchange(ref _head.Next, entry, entry.Next) != entry.Next);
        }

        internal ISlot Peek()
        {
            var entry = _head.Next;
            return entry != null ? entry.Slot : null;
        }

        internal bool Pop()
        {
            SlotEntry entry = null;
            do
            {
                entry = _head.Next;
                if (entry == null)
                {
                    return false;
                }
            } while (Interlocked.CompareExchange(ref _head.Next, entry.Next, entry) != entry);
            return true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets in snapshot.
        /// </summary>
        /// <param name="ctx">
        ///  The context.
        /// </param>
        /// <returns>
        ///  The in snapshot.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISlot GetInSnapshot(CommandContext ctx)
        {
            DebugContract.Requires(ctx);

            SlotEntry entry = _head.Next;
            while (entry != null)
            {
                var slot = entry.Slot;
                if (ctx.IsValidInSnapshot(slot))
                    return slot;
                entry = entry.Next;
            }
            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Renvoi le slot actif (le dernier non supprimé) - TODO je ne pense pas que ce soit bon.
        /// </summary>
        /// <returns>
        ///  The active slot.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISlot GetActiveSlot()
        {
            SlotEntry entry = _head.Next;
            while (entry != null)
            {
                var slot = entry.Slot;
                if (slot.XMax == null)
                    return slot;
                entry = entry.Next;
            }
            return null;
        }

        internal void Mark()
        {
            //_lastAccess = PreciseClock.GetCurrent();
            //Interlocked.Increment(ref _hits);
        }

        #region IDisposable Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            _head = null;
        }

        #endregion
    }
}