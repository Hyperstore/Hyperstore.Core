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

using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Hyperstore.Modeling.Scopes
{
    internal class DomainScope : DomainModel, IScope, IDomainScope
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="services">
        ///  The services.
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
        ///-------------------------------------------------------------------------------------------------
        public DomainScope(IServicesContainer services, string name, string extensionName, IDomainModel extendeDomainModel)
            : base(services, name)
        {
            DebugContract.Requires(services);
            DebugContract.RequiresNotEmpty(name);
            DebugContract.Requires(extendeDomainModel);
            DebugContract.RequiresNotEmpty(extensionName);

            ExtensionName = extensionName;
            ExtendedDomainModel = extendeDomainModel;
        }

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

        public IEnumerable<IModelElement> GetScopeElements(ISchemaElement schemaElement = null)
        {
            var graph = InnerGraph as IScopeHyperGraph;
            Debug.Assert(graph != null);
            return graph.GetExtensionElements(schemaElement);
        }

        public IEnumerable<GraphNode> GetDeletedElements()
        {
            var graph = InnerGraph as IScopeHyperGraph;
            Debug.Assert(graph != null);
            return graph.GetDeletedElements();
        }

        public IEnumerable<IModelRelationship> GetExtensionRelationships(ISchemaRelationship schemaRelationship = null, IModelElement start = null, IModelElement end = null)
        {
            return base.GetRelationships(schemaRelationship, start, end);
        }

        public System.Collections.Generic.IEnumerable<PropertyValue> GetUpdatedProperties()
        {
            var graph = InnerGraph as IScopeHyperGraph;
            Debug.Assert(graph != null);
            return graph.GetUpdatedProperties();
        }

        public override System.Threading.Tasks.Task<IDomainScope> CreateScopeAsync(string extensionName, IDomainConfiguration configuration = null)
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
            return new ScopeHyperGraph(Services, ExtendedDomainModel as IHyperGraphProvider);
        }
    }
}