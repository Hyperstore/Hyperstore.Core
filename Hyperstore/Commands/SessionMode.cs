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