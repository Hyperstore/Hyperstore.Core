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