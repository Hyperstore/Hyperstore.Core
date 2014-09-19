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

using Hyperstore.Modeling.Metadata.Constraints;
using Hyperstore.Modeling.Scopes;

#endregion

namespace Hyperstore.Modeling
{
    internal class ExtensionSchemaDefinition : SchemaDefinition
    {
        private readonly ISchema _extendedSchema;
        private readonly ISchemaDefinition _definition;
        private readonly SchemaConstraintExtensionMode _mode;

        internal ExtensionSchemaDefinition(ISchemaDefinition definition, ISchema extendedSchema, SchemaConstraintExtensionMode mode)
            : base(definition.SchemaName)
        {
            DebugContract.Requires(extendedSchema, "extendedSchema");
            DebugContract.Requires(definition, "definition");

            _definition = definition;
            _extendedSchema = extendedSchema;
            _mode = mode;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [schema loaded].
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected override void OnSchemaLoaded(ISchema schema)
        {
            _definition.OnSchemaLoaded(schema);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Defines the schema.
        /// </summary>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected override void DefineSchema(ISchema schema)
        {
            _definition.DefineSchema(schema);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare services container.
        /// </summary>
        /// <param name="container">
        ///  The default services container.
        /// </param>
        /// <returns>
        ///  An services container.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override IServicesContainer PrepareScopedContainer(IServicesContainer container)
        {
            return _definition.PrepareScopedContainer(container);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Creates a schema. </summary>
        /// <param name="services"> The domain services. </param>
        /// <returns>   The new schema. </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override ISchema CreateSchema(IServicesContainer services)
        {
            return new Hyperstore.Modeling.Scopes.DomainSchemaExtension(_extendedSchema,
                                                                              services,
                                                                              Behavior,
                                                                              new ExtensionConstraintManager(services, _extendedSchema, _mode));
        }
    }
}