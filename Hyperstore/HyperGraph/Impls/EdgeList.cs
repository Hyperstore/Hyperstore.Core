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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Liste des relations au niveau d'un noeud.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.HyperGraph.IEdgeList"/>
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{Hyperstore.Modeling.HyperGraph.EdgeInfo}"/>
    ///-------------------------------------------------------------------------------------------------
    public class EdgeList : IEdgeList, IEnumerable<EdgeInfo>
    {
        private readonly ImmutableDictionary<Identity, EdgeInfo> _edges;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public EdgeList()
        {
            _edges = ImmutableDictionary<Identity, EdgeInfo>.Empty;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructeur de copie.
        /// </summary>
        /// <param name="list">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public EdgeList(ImmutableDictionary<Identity, EdgeInfo> list)
        {
            DebugContract.Requires(list, "list");
            _edges = list; // new ImmutableDictionary<Identity, EdgeInfo>(infos._edges);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds node.
        /// </summary>
        /// <param name="info">
        ///  The information to add.
        /// </param>
        /// <returns>
        ///  An EdgeList.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public EdgeList Add(EdgeInfo info)
        {
            if (!_edges.ContainsKey(info.Id))
                return new EdgeList(_edges.Add(info.Id, info));
            return new EdgeList(_edges);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes the by key described by ID.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  An EdgeList.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public EdgeList RemoveByKey(Identity id)
        {
            Contract.Requires(id, "id");
            return new EdgeList(_edges.Remove(id));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the number of.
        /// </summary>
        /// <value>
        ///  The count.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Count
        {
            get { return _edges.Count; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the enumerator.
        /// </summary>
        /// <returns>
        ///  The enumerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerator<EdgeInfo> GetEnumerator()
        {
            return _edges.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _edges.Values.GetEnumerator();
        }
    }
}