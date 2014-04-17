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