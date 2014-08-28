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
using System.Linq;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Validations;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Domain;
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