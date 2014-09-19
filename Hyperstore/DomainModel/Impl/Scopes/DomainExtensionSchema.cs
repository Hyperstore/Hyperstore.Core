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
using System.Linq;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Metadata.Constraints;
#endregion

namespace Hyperstore.Modeling.Scopes
{
    internal class DomainSchemaExtension : DomainSchema, IScope, ISchemaExtension
    {
        private readonly ISchema _extendedMetaModel;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="extendedMetaModel">
        ///  The extended meta model.
        /// </param>
        /// <param name="services">
        ///  The services container.
        /// </param>
        /// <param name="behavior">
        ///  (Optional) the behavior.
        /// </param>
        /// <param name="constraints">
        ///  (Optional)
        ///  The constraints.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainSchemaExtension(ISchema extendedMetaModel, IServicesContainer services, DomainBehavior behavior = DomainBehavior.Standard, IConstraintsManager constraints = null)
            : base(extendedMetaModel.Name, services, behavior, constraints)
        {
            DebugContract.Requires(extendedMetaModel);
            DebugContract.Requires(services);
            DebugContract.Requires(constraints);

            _extendedMetaModel = extendedMetaModel;
        }

        protected override IHyperGraph ResolveHyperGraph()
        {
            return new ScopeHyperGraph(Services, _extendedMetaModel as IHyperGraphProvider);
        }
    }
}