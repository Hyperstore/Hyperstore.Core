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

using Hyperstore.Modeling.Traversal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A traversal builder.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public sealed class TraversalBuilder
    {
        private class Evaluator : ITraversalVisitor
        {
            private Func<GraphPath, GraphTraversalEvaluatorResult> _pathEvaluator;

            internal Evaluator(Func<GraphPath, GraphTraversalEvaluatorResult> pathEvaluator)
            {
                DebugContract.Requires(pathEvaluator);
                _pathEvaluator = pathEvaluator;
            }

            GraphTraversalEvaluatorResult ITraversalVisitor.Visit(GraphPath path)
            {
                if (_pathEvaluator == null)
                    return GraphTraversalEvaluatorResult.IncludeAndContinue;

                return _pathEvaluator(path);
            }

        }

        private ITraversalQuery _query;

        internal TraversalBuilder(IDomainModel domain)
        {
            _query = new TraversalQuery(domain);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unicity policy.
        /// </summary>
        /// <param name="policy">
        ///  The policy.
        /// </param>
        /// <returns>
        ///  A TraversalBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TraversalBuilder UnicityPolicy(IGraphTraversalUnicityPolicy policy)
        {
            Contract.Requires(policy, "policy");
            _query.UnicityPolicy = policy;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Node iterator.
        /// </summary>
        /// <param name="iterator">
        ///  The iterator.
        /// </param>
        /// <returns>
        ///  A TraversalBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TraversalBuilder NodeIterator(INodeIncidenceIterator iterator)
        {
            Contract.Requires(iterator, "iterator");
            _query.IncidencesIterator = iterator;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the every path action.
        /// </summary>
        /// <param name="visitor">
        ///  The visitor.
        /// </param>
        /// <returns>
        ///  A TraversalBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TraversalBuilder OnEveryPath(Func<GraphPath, GraphTraversalEvaluatorResult> visitor)
        {
            Contract.Requires(visitor, "visitor");
            _query.Evaluator = new Evaluator(visitor);
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  With visitor.
        /// </summary>
        /// <param name="visitor">
        ///  The visitor.
        /// </param>
        /// <returns>
        ///  A TraversalBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TraversalBuilder WithVisitor(ITraversalVisitor visitor)
        {
            Contract.Requires(visitor, "visitor");
            _query.Evaluator = visitor;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Path traverser.
        /// </summary>
        /// <param name="traverser">
        ///  The traverser.
        /// </param>
        /// <returns>
        ///  A TraversalBuilder.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public TraversalBuilder PathTraverser(IGraphPathTraverser traverser)
        {
            Contract.Requires(traverser, "traverser");
            _query.PathTraverser = traverser;
            return this;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Traverses the given node.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="node">
        ///  The node.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Traverse(NodeInfo node)
        {
            Contract.Requires(node, "node");
            
            if (_query.Evaluator == null)
                throw new Exception("You must define a visitor");

            _query.GetPaths(node).ToList();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the paths in this collection.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the paths in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<GraphPath> GetPaths(NodeInfo node)
        {
            Contract.Requires(node, "node");

            return _query.GetPaths(node);
        }
    }
}
