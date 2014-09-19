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
using System.Text;

#endregion

namespace Hyperstore.Modeling.Statistics
{
    internal sealed class StatisticCategory
    {
        private readonly Dictionary<string, IStatisticCounter> _counters = new Dictionary<string, IStatisticCounter>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Indexer to get items within this collection using array index syntax.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The indexed item.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IStatisticCounter this[string name]
        {
            get { return _counters[name]; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the counter.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="description">
        ///  The description.
        /// </param>
        /// <param name="counterType">
        ///  Type of the counter.
        /// </param>
        /// <returns>
        ///  An IStatisticCounter.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IStatisticCounter RegisterCounter(string name, string description, StatisticCounterType counterType)
        {
            if (_counters.ContainsKey(name))
                return _counters[name];

            var counter = new StatisticCounter(description, counterType);
            _counters.Add(name, counter);
            return counter;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Displays this instance.
        /// </summary>
        /// <returns>
        ///  A string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public string Display()
        {
            var sb = new StringBuilder();
            foreach (var counter in _counters)
            {
                sb.AppendFormat("Counter {0} {1}", counter.Key, counter.Value);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}