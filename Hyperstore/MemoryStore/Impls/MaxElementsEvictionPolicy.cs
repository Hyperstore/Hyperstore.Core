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
using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.MemoryStore
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A maximum elements eviction policy.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IEvictionPolicy"/>
    ///-------------------------------------------------------------------------------------------------
    public class MaxElementsEvictionPolicy : IEvictionPolicy
    {
        private readonly int _max;
        private readonly int _minLifeTime;
        private readonly IHyperstore _store;
        private int _countDown;
        private HashSet<object> _removedNodes;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="services">
        ///  Services container
        /// </param>
        /// <param name="max">
        ///  Number of element indicating when the eviction process begins
        /// </param>
        /// <param name="minLifeTimeInMs">
        ///  (Optional)Do not evict element which has been created in this last delay (Default = 10 000ms)
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public MaxElementsEvictionPolicy(IServicesContainer services, int max, int minLifeTimeInMs = 10000)
        {
            Contract.Requires(max > 0, "max");
            Contract.Requires(minLifeTimeInMs >= 0, "minLifeTimeInMs");
            Contract.Requires(services, "services");

            _store = services.Resolve<IHyperstore>();
            _max = max;
            _minLifeTime = minLifeTimeInMs;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in ElementEvicted events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event EventHandler<ElementEvictedEventArgs> ElementEvicted;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Test indiquant si la valeur courante doit être sortie du cache.
        /// </summary>
        /// <param name="key">
        ///  .
        /// </param>
        /// <param name="slots">
        ///  .
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IEvictionPolicy.ShouldEvictSlot(object key, ISlotList slots)
        {
            // On a atteint le nbre max d'éviction
            if (_countDown == 0)
                return false;

            var isAPropertyValue = false;
            if (slots.ElementType == NodeType.Property)
            {
                isAPropertyValue = true;

                // On supprime les propriétés si leur noeud parent a été supprimé
                // On peut fonctionner ainsi car les slots propriétaires se situent toujours avant ds la liste des valeurs
                if (_removedNodes.Contains(slots.OwnerKey))
                {
                    _countDown--;
                    return true;
                }
            }

            if (_store.LockManager.HasLock(key) != LockType.None)
                return false;

            var delay = PreciseClock.CalculateEllapseTimeFrom(slots.LastAccess);
            if (delay > _minLifeTime)
            {
                _countDown--;
                if (!isAPropertyValue)
                    _removedNodes.Add(key); // Stockage des noeuds pour permettre la suppression de leurs propriétés

                OnElementEvicted(EvictionProcess.Eviction, key);
                return true;
            }

            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Démarrage du processus de purge (=vacuum)
        /// </summary>
        /// <param name="actualElementsCount">
        ///  Number of actual elements.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void IEvictionPolicy.StartProcess(int actualElementsCount)
        {
            // Calcul du nbre de valeurs devant être supprimées
            _countDown = actualElementsCount - _max;
            if (_countDown > 0)
            {
                _removedNodes = new HashSet<object>();
                OnElementEvicted(EvictionProcess.Start);
            }
            else // Pas d'éviction pour l'instant
                _countDown = 0;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Process the terminated.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void IEvictionPolicy.ProcessTerminated()
        {
            if (_removedNodes != null)
            {
                _removedNodes = null;
                OnElementEvicted(EvictionProcess.End);
            }
        }

        private void OnElementEvicted(EvictionProcess status, object key = null)
        {
            var tmp = ElementEvicted;
            if (tmp != null)
                tmp(this, new ElementEvictedEventArgs(status, key));
        }
    }
}