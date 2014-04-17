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
 
using Hyperstore.Modeling.HyperGraph.Adapters;
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
        public IndexDefinition(MemoryGraphAdapter graph, string name, ISchemaElement metaClass, bool unique, params string[] propertyNames)
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