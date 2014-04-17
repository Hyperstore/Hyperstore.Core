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
 
namespace Hyperstore.Modeling.Statistics
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for statistic counter.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IStatisticCounter
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the type of the counter.
        /// </summary>
        /// <value>
        ///  The total number of er type.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        StatisticCounterType CounterType { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the description.
        /// </summary>
        /// <value>
        ///  The description.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string Description { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the value.
        /// </summary>
        /// <value>
        ///  The value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        double Value { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Decs this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Dec();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Incrs this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Incr();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Increment by.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void IncrBy(long value);
    }
}