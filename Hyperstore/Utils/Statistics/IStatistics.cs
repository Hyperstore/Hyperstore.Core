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