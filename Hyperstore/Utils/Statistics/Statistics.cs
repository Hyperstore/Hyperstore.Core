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
    internal class EmptyStatistics : IStatistics
    {
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