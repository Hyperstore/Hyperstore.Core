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

using System;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Hyperstore.Modeling.Statistics
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A statistic counter.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Statistics.IStatisticCounter"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("{Description} {Value}")]
    public sealed class StatisticCounter : IStatisticCounter
    {
        private long _counter;
        private long _value;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="description">
        ///  The description.
        /// </param>
        /// <param name="counterType">
        ///  (Optional) The total number of er type.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounter(string description, StatisticCounterType counterType = StatisticCounterType.Value)
        {
            CounterType = counterType;
            Description = description;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the type of the counter.
        /// </summary>
        /// <value>
        ///  The total number of er type.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public StatisticCounterType CounterType { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the description.
        /// </summary>
        /// <value>
        ///  The description.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Description { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Decs this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dec()
        {
            Interlocked.Decrement(ref _counter);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Incrs this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Incr()
        {
            Interlocked.Increment(ref _counter);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Increment by.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void IncrBy(long value)
        {
            Interlocked.Add(ref _value, value);
            Interlocked.Increment(ref _counter);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public double Value
        {
            get
            {
                switch (CounterType)
                {
                    case StatisticCounterType.Value:
                        return _counter;
                    case StatisticCounterType.Average:
                        if (_counter != 0)
                            return _value/_counter;
                        break;
                }

                return 0;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("{0} : {1}", CounterType, Value);
        }
    }
}