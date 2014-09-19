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
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling
{
    /// <summary>
    /// </summary>
    internal interface ISessionInternal : ISessionInformation, IDisposable
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the session context.
        /// </summary>
        /// <value>
        ///  The session context.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        ISessionContext SessionContext { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Rejects the session (Abort).
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void RejectChanges();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enter in a new execution scope.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void PushExecutionScope();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Exit from a n execution scope.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void PopExecutionScope();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets the origin store identifier.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void SetOriginStoreId(Guid id);
    }
}