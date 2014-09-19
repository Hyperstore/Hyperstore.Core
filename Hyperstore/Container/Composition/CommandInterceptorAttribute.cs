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

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Attribute for command interceptor.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class CommandInterceptorAttribute : Hyperstore.Modeling.Container.Composition.HyperstoreAttribute, ICommandInterceptorMetadata
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  (Optional) The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CommandInterceptorAttribute(string domainModel = null) : base(domainModel)
        {
            Priority = 0;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the priority.
        /// </summary>
        /// <value>
        ///  The priority.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Priority { get; set; }
    }
}

