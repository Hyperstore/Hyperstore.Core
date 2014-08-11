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

using System.Collections.Generic;
using Hyperstore.Modeling.MemoryStore;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Key value storage provider Permet de stocker une donn�e sous la forme cl�/valeur. La donn�e
    ///  stock�e contient 2 meta donn�es supl�mentaires :
    ///  - ElementType : Qui permet d'optimiser les lectures en filtrant le type de la donn�e souhait�
    ///  (Noeud, relation, propri�t�)
    ///  - OwnerKey : Dans le cas du stockage d'une propri�t�, on conserve une r�f�rence vers la
    ///  valeur correspond au propri�taire de cette propri�t� Ceci est particuli�rement important pour
    ///  les strat�gies d'eviction de cache.
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
        IGraphNode GetNode(Identity key);

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
        void AddNode(IGraphNode value, Identity ownerKey = null);

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
        void UpdateNode(IGraphNode value);

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
        IEnumerable<IGraphNode> GetAllNodes(NodeType elementType);

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