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