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

using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Validations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal class DomainModelExtension : DomainModel, IExtension, IDomainModelExtension
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="extensionName">
        ///  Name of the extension.
        /// </param>
        /// <param name="extendeDomainModel">
        ///  The extende domain model.
        /// </param>
        /// <param name="extensionMode">
        ///  The extension mode.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainModelExtension(IDependencyResolver resolver, string name, string extensionName, IDomainModel extendeDomainModel, ExtendedMode extensionMode)
            : base(resolver, name)
        {
            DebugContract.Requires(resolver);
            DebugContract.RequiresNotEmpty(name);
            DebugContract.Requires(extendeDomainModel);
            DebugContract.RequiresNotEmpty(extensionName);

            ExtensionName = extensionName;
            ExtendedDomainModel = extendeDomainModel;
            ExtensionMode = extensionMode;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the extension mode.
        /// </summary>
        /// <value>
        ///  The extension mode.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ExtendedMode ExtensionMode { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the extended domain model.
        /// </summary>
        /// <value>
        ///  The extended domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel ExtendedDomainModel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve identifier generator.
        /// </summary>
        /// <returns>
        ///  An IIdGenerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override IIdGenerator ResolveIdGenerator()
        {
            return ExtendedDomainModel.IdGenerator;
        }

        public IEnumerable<IModelElement> GetExtensionElements(ISchemaElement schemaElement = null)
        {
            var graph = InnerGraph as IExtensionHyperGraph;
            Debug.Assert(graph != null);
            return graph.GetExtensionElements(schemaElement);
        }

        public IEnumerable<INodeInfo> GetDeletedElements()
        {
            var graph = InnerGraph as IExtensionHyperGraph;
            Debug.Assert(graph != null);
            return graph.GetDeletedElements();
        }

        public IEnumerable<IModelRelationship> GetExtensionRelationships(ISchemaRelationship schemaRelationship = null, IModelElement start = null, IModelElement end = null)
        {
            return base.GetRelationships(schemaRelationship, start, end);
        }

        public override System.Threading.Tasks.Task<IDomainModelExtension> LoadExtensionAsync(string extensionName, ExtendedMode mode, IDomainConfiguration configuration = null)
        {
            throw new NotImplementedException();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve hyper graph.
        /// </summary>
        /// <returns>
        ///  An IHyperGraph.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override IHyperGraph ResolveHyperGraph()
        {
            return new DomainExtensionHyperGraph(DependencyResolver, ExtendedDomainModel as IHyperGraphProvider, ExtensionMode);
        }
    }
}