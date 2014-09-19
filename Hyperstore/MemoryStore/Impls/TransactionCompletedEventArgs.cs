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