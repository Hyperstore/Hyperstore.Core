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
    ///  Interface for statistics.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IStatistics
    {
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
        IStatisticCounter GetCounter(string category, string name);

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
        IStatisticCounter RegisterCounter(string category, string name, string description, StatisticCounterType counterType);
    }
}