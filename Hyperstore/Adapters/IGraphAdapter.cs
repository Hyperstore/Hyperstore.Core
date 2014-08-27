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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Adapters
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for graph adapter.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IGraphAdapter
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates load nodes in this collection.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process load nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<QueryNodeResult> LoadNodes(Query query);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for supports lazy loading.
    /// </summary>
    /// <seealso cref="T:IGraphAdapter"/>
    ///-------------------------------------------------------------------------------------------------
    public interface ISupportsLazyLoading : IGraphAdapter
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates lazy loading nodes in this collection.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process lazy loading nodes in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<QueryNodeResult> LazyLoadingNodes(Query query);

    }
}
