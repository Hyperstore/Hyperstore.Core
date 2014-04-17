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
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal class DomainExtensionHyperGraph : HyperGraph.HyperGraph
    {
        private readonly IDomainModel _extendedDomain;
        private readonly ExtendedMode _extensionMode;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <param name="extendedDomain">
        ///  The extended domain.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainExtensionHyperGraph(IDependencyResolver resolver, IDomainModel extendedDomain, ExtendedMode mode) : base(resolver)
        {
            DebugContract.Requires(resolver);
            DebugContract.Requires(extendedDomain);

            _extendedDomain = extendedDomain;
            _extensionMode = mode;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve graph adapter.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <returns>
        ///  An ICacheAdapter.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override ICacheAdapter ResolveGraphAdapter()
        {
            var adapter = base.ResolveGraphAdapter();
            if (adapter == null)
                throw new Exception(ExceptionMessages.CantExtendDomainModel_GraphAdapter);

            var innerGraph = _extendedDomain.Resolve<IHyperGraph>();
            var extendedDomainModelAdapter = innerGraph.Adapter;
            if (extendedDomainModelAdapter == null)
                throw new Exception(ExceptionMessages.CantExtendDomainModel_GraphAdapter);

            return new DomainExtensionAdapter(adapter, extendedDomainModelAdapter, _extensionMode);
        }
    }
}