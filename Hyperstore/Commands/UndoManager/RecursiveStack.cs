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
 
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hyperstore.Modeling.Commands
{
    internal class RecursiveStack<T> : IEnumerable<T>
    {
        /// <summary>
        /// Queue Capacity
        /// </summary>
        private readonly int _maxElements;

        /// <summary>
        /// Queue position
        /// </summary>
        /// Implementing with a circular buffer, no memory realloc
        /// ! . . . . . . . . . . . ! Max elements
        ///     ^         ^
        ///    start     end
        private int _start;
        private int _end;
        private bool _full;
        private readonly T[] _elements;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="size">
        ///  The size.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public RecursiveStack(int size)
        {
            Contract.Requires(size > 0, "size");
            _maxElements = size;
            _elements = new T[size];
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of. 
        /// </summary>
        /// <value>
        ///  The count.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Count
        {
            get
            {
                var size = 0;

                if (_end < _start)
                {
                    size = _maxElements - _start + _end;
                }
                else if (_end == _start)
                {
                    size = _full ? _maxElements : 0;
                }
                else
                {
                    size = _end - _start;
                }
                return size;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns the top-of-stack object without removing it.
        /// </summary>
        /// <returns>
        ///  The current top-of-stack object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T Peek()
        {
            return Count == 0 ? default(T) : this._elements[(this._end == 0 ? this._maxElements : this._end) - 1];
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Clears this instance to its blank/initial state.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Clear()
        {
            _full = false;
            _start = _end = 0;
            for (int i = 0; i < _maxElements; i++)
            {
                _elements[i] = default(T);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes and returns the top-of-stack object.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <returns>
        ///  The previous top-of-stack object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T Pop()
        {
            if (Count == 0)
                throw new Exception(ExceptionMessages.EmptyStack);
            _end--;
            if (_end < 0)
            {
                _end = _maxElements - 1;
            }

            var element = _elements[_end];
            _elements[_end] = default(T);

            _full = false;
            return element;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Pushes an object onto this stack.
        /// </summary>
        /// <param name="element">
        ///  The element to push.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Push(T element)
        {
            Contract.Requires(element != null, "element");

            if (Count == _maxElements)
            {
                RemoveElement();
            }

            _elements[_end++] = element;
            if (_end == _maxElements)
            {
                _end = 0;
            }
            _full = _end == _start;
        }

        private void RemoveElement()
        {
            _elements[_start++] = default(T);
            if (_start >= _maxElements)
            {
                _start = 0;
            }
            _full = false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerator<T> GetEnumerator()
        {
            var cx = Count;
            var pos = _start;
            while (cx-- > 0)
            {
                yield return _elements[pos++];
                if (pos == _maxElements)
                    pos = 0;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
