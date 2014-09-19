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
 
namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A setting.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class Setting
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The memory store vacuum interval in seconds.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public const string MemoryStoreVacuumIntervalInSeconds = "MemoryStoreVacuumIntervalInSeconds";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The max time before deadlock in seconds.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public const string MaxTimeBeforeDeadlockInMs = "MaxTimeBeforeDeadlockInMs";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="Setting" /> class.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public Setting(string name, object value)
        {
            Contract.RequiresNotEmpty(name, "name");
            Name = name;
            Value = value;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name.
        /// </summary>
        /// <value>
        ///  The name.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Name { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public object Value { get; private set; }
    }
}