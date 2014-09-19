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