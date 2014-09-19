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
 
namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Values that represent TrackingState.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public enum TrackingState
    {
        /// <summary>
        /// Unknow element - Should be remove from cache by an eviction policy)
        /// </summary>
        Unknown,
        /// <summary>
        /// This element has been added
        /// </summary>
        Added,
        /// <summary>
        /// This element has been updated
        /// </summary>
        Updated,
        /// <summary>
        /// This element has been removed
        /// </summary>
        Removed
    }
}