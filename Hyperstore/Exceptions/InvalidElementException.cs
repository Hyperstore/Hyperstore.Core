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
    ///  Exception for signalling invalid element errors.
    /// </summary>
    /// <seealso cref="T:System.Exception"/>
    ///-------------------------------------------------------------------------------------------------
    public class InvalidElementException : Exception
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="message">
        ///  (Optional) The error message that explains the reason for the exception, or an empty
        ///  string("").
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public InvalidElementException(Identity id, string message = null) : base(message)
        {
            Id = id;            
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a message that describes the current exception.
        /// </summary>
        /// <value>
        ///  The error message that explains the reason for the exception, or an empty string("").
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public override string Message
        {
            get
            {
                return base.Message ?? String.Format(ExceptionMessages.InvalidElementFormat, Id);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }
    }
}