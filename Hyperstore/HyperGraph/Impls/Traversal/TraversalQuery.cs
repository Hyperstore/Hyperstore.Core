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

#region Imports (6)

#endregion Imports (6)

#region Imports

using System;
using System.Collections.Generic;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling.Traversal
{
    internal class TraversalQuery : ITraversalQuery
    {
        #region Classes of TraversalQuery (3)

        private class AllEvaluator : ITraversalVisitor
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Returns what to do with this path in a traversal query.
            /// </summary>
            /// <param name="path">
            ///  Full pathname of the file.
            /// </param>
            /// <returns>
            ///  A GraphTraversalEvaluatorResult.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public GraphTraversalEvaluatorResult Visit(GraphPath path)
            {
                return GraphTraversalEvaluatorResult.IncludeAndContinue;
            }
        }

        private class DefaultIncidencesIterator : INodeIncidenceIterator
        {
            private Hyperstore.Modeling.HyperGraph.HyperGraph _hypergraph;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the direction.
            /// </summary>
            /// <value>
            ///  The direction.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public Direction Direction { get; private set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="direction">
            ///  The direction.
            /// </param>
            /// <param name="hypergraph">
            ///  The hypergraph.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public DefaultIncidencesIterator(Direction direction, Hyperstore.Modeling.HyperGraph.HyperGraph hypergraph)
            {
                _hypergraph = hypergraph;
                Direction = direction;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Get a relationship list.
            /// </summary>
            /// <param name="node">
            ///  The mel.
            /// </param>
            /// <returns>
            ///  An enumerator that allows foreach to be used to process from in this collection.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public IEnumerable<EdgeInfo> From(Identity node)
            {
                GraphNode graphNode;
                if (!_hypergraph.GetGraphNode(node, NodeType.EdgeOrNode, out graphNode))
                    yield break;

                foreach (var rel in _hypergraph.GetGraphEdges(graphNode, Direction))
                {
                    if (rel != null)
                        yield return rel;
                }
            }
        }

        #endregion Classes of TraversalQuery (3)

        #region Properties of TraversalQuery (7)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the paths in this collection.
        /// </summary>
        /// <param name="node">
        ///  The start node.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the paths in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<GraphPath> GetPaths(NodeInfo node)
        {
            Contract.Requires(node, "node");

            try
            {
                PathTraverser.Initialize(this);
                UnicityPolicy.Reset();

                foreach (var p in PathTraverser.Traverse(node))
                {
                    yield return p;
                }
            }
            finally
            {
                UnicityPolicy.Reset();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the evaluator.
        /// </summary>
        /// <value>
        ///  The evaluator.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ITraversalVisitor Evaluator { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the incidences iterator.
        /// </summary>
        /// <value>
        ///  The incidences iterator.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public INodeIncidenceIterator IncidencesIterator { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the path traverser.
        /// </summary>
        /// <value>
        ///  The path traverser.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IGraphPathTraverser PathTraverser { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the unicity policy.
        /// </summary>
        /// <value>
        ///  The unicity policy.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IGraphTraversalUnicityPolicy UnicityPolicy { get; set; }

        #endregion Properties of TraversalQuery (7)

        internal TraversalQuery(IDomainModel domain)
        {
            DomainModel = domain;
            PathTraverser = new GraphBreadthFirstTraverser();
            Evaluator = new AllEvaluator();
            UnicityPolicy = new GlobalNodeUnicity();

            var provider = domain as Hyperstore.Modeling.Domain.IHyperGraphProvider;
            System.Diagnostics.Debug.Assert(provider != null);
            IncidencesIterator = new DefaultIncidencesIterator(Direction.Outgoing, provider.InnerGraph as HyperGraph.HyperGraph);
        }

    }
}