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

using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Session result
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public interface ISessionResult
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether there is no error messages.
        /// </summary>
        /// <value>
        ///  <c>true</c> if [has errors]; otherwise, <c>false</c>.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool HasErrors { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance has warnings.
        /// </summary>
        /// <value>
        ///  true if this instance has warnings, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        bool HasWarnings { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the messages.
        /// </summary>
        /// <value>
        ///  The messages.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<DiagnosticMessage> Messages { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Set the current session in silent mode. No exception will be raised at the end of the session.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void SetSilentMode();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Raises the NotifyDataError event for all element involved during this execution process.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void NotifyDataErrors();
    }
}