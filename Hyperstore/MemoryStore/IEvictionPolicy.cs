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