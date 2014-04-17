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
using System.Diagnostics;
using System.Threading;

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    [DebuggerDisplay("Value={Value}")]
    internal class Slot<TValue> : ISlot
    {
        private static long _sequence;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public Slot(TValue value)
        {
            Value = value;
            Id = Interlocked.Increment(ref _sequence); // Always > 0
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public TValue Value { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Transaction ayant créée ou modifiée ce slot.
        /// </summary>
        /// <value>
        ///  The minimum value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long XMin { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Transaction ayant supprimé ce slot (ou via un update)
        /// </summary>
        /// <value>
        ///  The maximum value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long? XMax { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  N° de la commande dans la transaction qui a modifiée ce slot (add/update/delete)
        /// </summary>
        /// <value>
        ///  The minimum value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int CMin { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Identifiant unique du slot.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long Id { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("XMin={0}, XMax={1}, CMin={2}", XMin, XMax, CMin);
        }
    }
}