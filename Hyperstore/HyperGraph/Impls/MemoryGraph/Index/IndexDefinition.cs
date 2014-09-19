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
 
namespace Hyperstore.Modeling.HyperGraph.Index
{
    internal class IndexDefinition
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="graph">
        ///  The graph.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="metaClass">
        ///  The meta class.
        /// </param>
        /// <param name="unique">
        ///  true to unique.
        /// </param>
        /// <param name="propertyNames">
        ///  A list of names of the properties.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public IndexDefinition(IHyperGraph graph, string name, ISchemaElement metaClass, bool unique, params string[] propertyNames)
        {
            DebugContract.Requires(graph);
            DebugContract.RequiresNotEmpty(name);
            DebugContract.Requires(metaClass);

            if (propertyNames.Length > 0)
            {
                PropertyNames = new string[propertyNames.Length];
                for (var i = 0; i < propertyNames.Length; i++)
                {
                    PropertyNames[i] = propertyNames[i];
                }
            }
            MetaClass = metaClass;
            Index = new BTreeIndex(graph, name, unique);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a list of names of the properties.
        /// </summary>
        /// <value>
        ///  A list of names of the properties.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string[] PropertyNames { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta class.
        /// </summary>
        /// <value>
        ///  The meta class.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement MetaClass { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the zero-based index of this instance.
        /// </summary>
        /// <value>
        ///  The index.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public BTreeIndex Index { get; private set; }

        internal bool IsImpactedBy(ISchemaElement metaclass, string propertyName)
        {
            DebugContract.Requires(metaclass);

            return metaclass.IsA(MetaClass) && (propertyName == null || (PropertyNames != null && PropertyNames[0] == propertyName));
        }
    }
}