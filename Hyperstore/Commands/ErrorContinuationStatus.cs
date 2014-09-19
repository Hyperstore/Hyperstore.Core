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
    ///  Values that represent ErrorContinuationStatus.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    public enum ErrorContinuationStatus
    {
        /// <summary>
        ///  Specifies the continue option.
        /// </summary>
        Continue,
        /// <summary>
        ///  Specifies the abort option.
        /// </summary>
        Abort,
        /// <summary>
        ///  Specifies the retry option.
        /// </summary>
        Retry
    }
}