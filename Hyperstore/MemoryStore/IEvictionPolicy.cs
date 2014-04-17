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
using Hyperstore.Modeling.MemoryStore;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Eviction policy for memory store.
    /// </summary>
    /// <remarks>
    ///  L'éviction est calculée à chaque vaccuum. Quand le process itére sur toutes les valeurs pour
    ///  purger les slots obsolètes, on en profite pour chercher les slots à purger.
    /// </remarks>
    ///-------------------------------------------------------------------------------------------------
    public interface IEvictionPolicy
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in ElementEvicted events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        event EventHandler<ElementEvictedEventArgs> ElementEvicted;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Démarrage du processus de purge (=vacuum)
        /// </summary>
        /// <param name="actualElementsCount">
        ///  Number of actual elements.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void StartProcess(int actualElementsCount);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Process the terminated.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void ProcessTerminated();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determine if we should evict slot.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="slots">
        ///  The slots.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool ShouldEvictSlot(object key, ISlotList slots);
    }
}