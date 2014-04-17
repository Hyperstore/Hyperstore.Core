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

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Completed transaction arguments.
    /// </summary>
    /// <seealso cref="T:System.EventArgs"/>
    ///-------------------------------------------------------------------------------------------------
    public class TransactionCompletedEventArgs : EventArgs
    {
        internal TransactionCompletedEventArgs(long transactionId, bool committed, bool nested)
        {
            Committed = committed;
            TransactionId = transactionId;
            Nested = nested;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Transaction terminated.
        /// </summary>
        /// <value>
        ///  The identifier of the transaction.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public long TransactionId { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether the committed.
        /// </summary>
        /// <value>
        ///  true if committed, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool Committed { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether the nested.
        /// </summary>
        /// <value>
        ///  true if nested, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool Nested { get; private set; }
    }
}