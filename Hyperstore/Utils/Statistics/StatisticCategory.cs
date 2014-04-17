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