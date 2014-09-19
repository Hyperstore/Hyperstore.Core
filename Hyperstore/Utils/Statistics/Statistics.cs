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
using System.Text;

#endregion

namespace Hyperstore.Modeling.Statistics
{
    internal class EmptyStatistics : IStatistics
    {
        private static Lazy<IStatistics> _instance = new Lazy<IStatistics>(() => new EmptyStatistics());

        public static IStatistics DefaultInstance { get { return _instance.Value; } }

        private EmptyStatistics()
        {
        }

        class EmptyCounter : IStatisticCounter
        {
            public static IStatisticCounter Empty = new EmptyCounter();

            public StatisticCounterType CounterType
            {
                get { throw new System.NotImplementedException(); }
            }

            public string Description
            {
                get { throw new System.NotImplementedException(); }
            }

            public double Value
            {
                get { throw new System.NotImplementedException(); }
            }

            public void Dec()
            {
            }

            public void Incr()
            {
            }

            public void IncrBy(long value)
            {
            }
        }

        public IStatisticCounter GetCounter(string category, string name)
        {
            return null;
        }

        public IStatisticCounter RegisterCounter(string category, string name, string description, StatisticCounterType counterType)
        {
            return EmptyCounter.Empty;
        }
    }
 
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A statistics.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Statistics.IStatistics"/>
    ///-------------------------------------------------------------------------------------------------
    public sealed class Statistics : IStatistics
    {
        private readonly Dictionary<string, StatisticCategory> _categories = new Dictionary<string, StatisticCategory>();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a counter.
        /// </summary>
        /// <param name="category">
        ///  The category.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  The counter.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IStatisticCounter GetCounter(string category, string name)
        {
            StatisticCategory cat;
            if (_categories.TryGetValue(category, out cat))
                return cat[name];
            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the counter.
        /// </summary>
        /// <param name="category">
        ///  The category.
        /// </param>
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
        public IStatisticCounter RegisterCounter(string category, string name, string description, StatisticCounterType counterType)
        {
            StatisticCategory cat;
            if (!_categories.TryGetValue(category, out cat))
            {
                cat = new StatisticCategory();
                _categories.Add(category, cat);
            }

            return cat.RegisterCounter(name, description, counterType);
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
            foreach (var cat in _categories)
            {
                sb.AppendFormat("Counters for category {0} :", cat.Key);
                sb.AppendLine();
                sb.Append(cat.Value.Display());
                sb.AppendLine("--------------------------------------------------------------------");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}