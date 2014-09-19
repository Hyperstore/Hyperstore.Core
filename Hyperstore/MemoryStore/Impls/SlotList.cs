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

using System.Collections;
using System.Collections.Generic;
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
    internal sealed class SlotList : IEnumerable<ISlot>, ISlotList
    {
        private readonly NodeType _elementType;
        private readonly object _ownerKey;
        private readonly List<ISlot> _slots;
        private int _hits;
        private long _lastAccess;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="clone">
        ///  The clone.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SlotList(SlotList clone)
        {
            DebugContract.Requires(clone, "clone");

            _ownerKey = clone._ownerKey;
            _elementType = clone._elementType;
            _hits = clone._hits;
            _lastAccess = clone.LastAccess;

            _ownerKey = clone._ownerKey;
            _slots = new List<ISlot>(Length + 2);
            _slots.AddRange(clone._slots.Where(s => s.Id > 0));
            Length = _slots.Count;
        }

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
            _slots = new List<ISlot>(12);
            Mark();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the length.
        /// </summary>
        /// <value>
        ///  The length.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Length { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerator<ISlot> GetEnumerator()
        {
            return new ReverseEnumerator(_slots);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ReverseEnumerator(_slots);
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds slot.
        /// </summary>
        /// <param name="slot">
        ///  The slot to remove.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Add(ISlot slot)
        {
            DebugContract.Requires(slot);

            this._slots.Add(slot);
            Length++;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the given slot.
        /// </summary>
        /// <param name="slot">
        ///  The slot to remove.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Remove(ISlot slot)
        {
            DebugContract.Requires(slot);

            // Pas de lock ici car le Remove est tjs appelé dans le contexte du vaccum qui
            // a un lock exclusif sur toutes les données
            for (var i = 0; i < _slots.Count; i++)
            {
                var s = _slots[i];
                if (s != null && s.Id == slot.Id)
                {
                    // On se contente de mettre à null
                    _slots[i] = null;
                    Length--;
                }
            }
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

            //    this._lock.EnterReadLock();
            var enumerator = GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    if (ctx.IsValidInSnapshot(item))
                        return item;
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
                //      this._lock.ExitReadLock();
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
            //this._lock.EnterReadLock();
            var enumerator = GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    var slot = enumerator.Current;
                    if (slot.XMax == null)
                        return slot;
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
                //    this._lock.ExitReadLock();
            }
            return null;
        }

        internal void Mark()
        {
            _lastAccess = PreciseClock.GetCurrent();
            Interlocked.Increment(ref _hits);
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
        }

        #endregion

        private class ReverseEnumerator : IEnumerator<ISlot>
        {
            private ISlot _current;
            private IList<ISlot> _list;
            private int _pos;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="list">
            ///  The list.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public ReverseEnumerator(IList<ISlot> list)
            {
                DebugContract.Requires(list);

                _list = list;
                _pos = list.Count;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the current.
            /// </summary>
            /// <value>
            ///  The current.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public ISlot Current
            {
                get { return _current; }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
            ///  resources.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Dispose()
            {
                _list = null;
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Determines if we can move next.
            /// </summary>
            /// <returns>
            ///  true if it succeeds, false if it fails.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public bool MoveNext()
            {
                if (_pos > 0)
                {
                    _pos--;
                    _current = _list[_pos];
                    return true;
                }
                _current = null;
                return false;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Resets this instance.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Reset()
            {
                _current = null;
            }
        }
    }
}