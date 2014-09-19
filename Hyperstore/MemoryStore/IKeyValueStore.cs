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

using System.Collections.Generic;
using Hyperstore.Modeling.MemoryStore;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Key value storage provider Permet de stocker une donnée sous la forme clé/valeur. La donnée
    ///  stockée contient 2 meta données suplémentaires :
    ///  - ElementType : Qui permet d'optimiser les lectures en filtrant le type de la donnée souhaité
    ///  (Noeud, relation, propriété)
    ///  - OwnerKey : Dans le cas du stockage d'une propriété, on conserve une référence vers la
    ///  valeur correspond au propriétaire de cette propriété Ceci est particulièrement important pour
    ///  les stratégies d'eviction de cache.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IKeyValueStore
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the eviction policy.
        /// </summary>
        /// <value>
        ///  The eviction policy.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEvictionPolicy EvictionPolicy { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Get a slot by key.
        /// </summary>
        /// <param name="key">
        ///  .
        /// </param>
        /// <returns>
        ///  The value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        GraphNode GetNode(Identity key);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Add a node.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="ownerKey">
        ///  (Optional) The owner key.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddNode(GraphNode value, Identity ownerKey = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Remove a value.
        /// </summary>
        /// <param name="key">
        ///  .
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool RemoveNode(Identity key);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Update a value.
        /// </summary>
        /// <param name="value">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void UpdateNode(GraphNode value);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Closes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        void Close();

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets all.
        /// </summary>
        /// <param name="elementType">
        ///  Type of the element.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphNode> GetAllNodes(NodeType elementType);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Exists the given identifier.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool Exists(Identity id);
    }
}