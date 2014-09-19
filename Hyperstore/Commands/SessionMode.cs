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

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Mode of the session
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum SessionMode
    {
        /// <summary>
        /// Normal mode
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Ignore all interceptors
        /// </summary>
        SkipInterceptors = 1,
        /// <summary>
        /// Ignore all constraints
        /// </summary>
        SkipConstraints = 2,
        /// <summary>
        /// Schema loading
        /// </summary>
        LoadingSchema = 3,
        /// <summary>
        /// A domain is being loading. If an adapter is attached to the domain this is disable the lazy loading mechanism.
        /// </summary>
        Loading = 4,
        /// <summary>
        /// Undo mode
        /// </summary>
        Undo = 8,
        /// <summary>
        /// Redo mode
        /// </summary>
        Redo = 16,
        /// <summary>
        /// Used to check undo/redo mode
        /// </summary>
        UndoOrRedo = 24,
        /// <summary>
        /// Domain is being serialized
        /// </summary>
        Serializing = 32,
        /// <summary>
        /// Silent mode - No exception will be raised by the session
        /// </summary>
        SilentMode = 64,
        /// <summary>
        /// No events will be raised
        /// </summary>
        SkipNotifications = 128,
        /// <summary>
        ///  Ignore L1 cache
        /// </summary>
        IgnoreCache=256
    }
}