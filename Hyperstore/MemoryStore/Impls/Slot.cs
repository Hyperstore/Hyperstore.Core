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