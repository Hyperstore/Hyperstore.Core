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