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