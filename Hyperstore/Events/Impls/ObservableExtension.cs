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
using System.Linq;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An observable extension.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class ObservableExtension
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  An IObservable&lt;T&gt; extension method that subscribes.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="source">
        ///  The source to act on.
        /// </param>
        /// <param name="onNext">
        ///  The on next.
        /// </param>
        /// <param name="onCompleted">
        ///  (Optional) the on completed.
        /// </param>
        /// <param name="onError">
        ///  (Optional) the on error.
        /// </param>
        /// <returns>
        ///  An IDisposable.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted = null, Action onError = null)
        {
            return source.Subscribe(new RelayObserver<T>(onNext, onCompleted, onError));
        }
    }
}