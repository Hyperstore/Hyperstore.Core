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

using System.Linq;
using System.Collections.Generic;
using Hyperstore.Modeling.Metadata.Constraints;

#endregion

namespace Hyperstore.Modeling.Scopes
{
    /// <summary>
    /// </summary>
    internal class ExtensionConstraintManager : ConstraintsManager
    {
        private readonly SchemaConstraintExtensionMode _extensionMode;
        private ISchema _extendedSchema;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="services">
        ///  The services.
        /// </param>
        /// <param name="extendedSchema">
        ///  The extended domain model.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ExtensionConstraintManager(IServicesContainer services, ISchema extendedSchema, SchemaConstraintExtensionMode mode) : base(services)
        {
            DebugContract.Requires(services);
            DebugContract.Requires(extendedSchema);

            _extensionMode = mode;
            _extendedSchema = extendedSchema;
        }

        public override ISessionResult CheckElements(IEnumerable<IModelElement> elements)
        {
            var result = base.CheckElements(elements) as IExecutionResultInternal;

            if (IsInMode(SchemaConstraintExtensionMode.Replace) || !(_extendedSchema is IConstraintManagerInternal))
                return result;

            var result2 = ((IConstraintManagerInternal)_extendedSchema.Constraints).CheckElements(elements) as IExecutionResultInternal;
            return result2.Merge(result);
        }

        public override ISessionResult Validate(IEnumerable<IModelElement> elements, string category = null)
        {
            var result = base.Validate(elements) as IExecutionResultInternal ;

            if (IsInMode(SchemaConstraintExtensionMode.Replace))
                return result;

            var result2 = _extendedSchema.Constraints.Validate(elements) as IExecutionResultInternal;
            return result2.Merge(result);
        }

        private bool IsInMode(SchemaConstraintExtensionMode mode)
        {
            return (_extensionMode & mode) == mode;
        }
    }
}