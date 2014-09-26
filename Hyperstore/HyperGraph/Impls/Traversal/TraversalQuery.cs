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
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A traversal query.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Traversal.IGraphTraversalConfiguration"/>
    ///-------------------------------------------------------------------------------------------------
    internal class TraversalQuery : ITraversalQuery
    {
        #region Classes of TraversalQuery (3)

        private class ActionsEvaluator : ITraversalVisitor
        {
            #region Properties of ActionsEvaluator (2)

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Sets the path evaluator.
            /// </summary>
            /// <value>
            ///  The path evaluator.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public Func<GraphPath, GraphTraversalEvaluatorResult> PathEvaluator { private get; set; }

            #endregion Properties of ActionsEvaluator (2)

            #region Methods of ActionsEvaluator (2)

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
                if (PathEvaluator == null)
                    return GraphTraversalEvaluatorResult.IncludeAndContinue;

                return PathEvaluator(path);
            }

            #endregion Methods of ActionsEvaluator (2)
        }

        private class AllEvaluator : ITraversalVisitor
        {
            #region Methods of AllEvaluator (2)

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

            #endregion Methods of AllEvaluator (2)
        }

        private class DefaultIncidencesIterator : INodeIncidenceIterator
        {
            #region Properties of DefaultIncidencesIterator (1)

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets the direction.
            /// </summary>
            /// <value>
            ///  The direction.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public Direction Direction { get; private set; }

            #endregion Properties of DefaultIncidencesIterator (1)

            #region Constructors of DefaultIncidencesIterator (1)

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="direction">
            ///  The direction.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public DefaultIncidencesIterator(Direction direction)
            {
                Direction = direction;
            }

            #endregion Constructors of DefaultIncidencesIterator (1)

            #region Methods of DefaultIncidencesIterator (1)

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Get a relationship list.
            /// </summary>
            /// <param name="mel">
            ///  The mel.
            /// </param>
            /// <returns>
            ///  An enumerator that allows foreach to be used to process from in this collection.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public IEnumerable<IModelRelationship> From(IModelElement mel)
            {
                if ((Direction.Outgoing & Direction) == Direction.Outgoing)
                {
                    foreach (var rel in mel.GetRelationships())
                    {
                        yield return rel;
                    }
                }

                if ((Direction & Direction.Incoming) == Direction.Incoming)
                {
                    foreach (var rel in mel.DomainModel.GetRelationships(end: mel))
                    {
                        yield return rel;
                    }
                }
            }

            #endregion Methods of DefaultIncidencesIterator (1)
        }

        #endregion Classes of TraversalQuery (3)

        #region Enums of TraversalQuery (2)

        #endregion Enums of TraversalQuery (2)

        #region Properties of TraversalQuery (7)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the full pathname of the create file.
        /// </summary>
        /// <value>
        ///  The full pathname of the create file.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Func<GraphPosition, GraphPath, GraphPath> CreatePath { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the paths in this collection.
        /// </summary>
        /// <param name="startNode">
        ///  The start node.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the paths in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<GraphPath> GetPaths(IModelElement startNode)
        {
            Contract.Requires(startNode, "startNode");

            try
            {
                PathTraverser.Initialize(this);
                UnicityPolicy.Reset();

                foreach (var p in PathTraverser.Traverse(startNode))
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

        #region Constructors of TraversalQuery (1)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public TraversalQuery()
        {
            PathTraverser = new GraphBreadthFirstTraverser();
            Evaluator = new AllEvaluator();
            UnicityPolicy = new GlobalNodeUnicity();
            IncidencesIterator = new DefaultIncidencesIterator(Direction.Outgoing);
        }

        #endregion Constructors of TraversalQuery (1)

        #region Methods of TraversalQuery (6)

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets the evaluator factory.
        /// </summary>
        /// <value>
        ///  The evaluator factory.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Func<GraphPath, GraphTraversalEvaluatorResult> EvaluatorFactory
        {
            set
            {
                var eval = Evaluator as ActionsEvaluator ?? new ActionsEvaluator();
                eval.PathEvaluator = value;
                Evaluator = eval;
            }
        }

        //public IGraphTraversalConfiguration RelationshipEvaluator(Func<IModelRelationship, bool> action)
        //{
        //    var eval = this.Evaluator as ActionsEvaluator;
        //    if (eval == null)
        //        eval = new ActionsEvaluator();
        //    eval.RelationshipEvaluator = action;
        //    this.Evaluator = eval;
        //    return this;
        //}

        //public IGraphTraversalConfiguration Relationships(Identity id)
        //{
        //    var eval = this.Evaluator as ActionsEvaluator;
        //    if (eval == null)
        //        eval = new ActionsEvaluator();
        //    eval.RelationshipEvaluator = rel => rel.Id == id;
        //    this.Evaluator = eval;
        //    return this;
        //}

        //public IGraphTraversalConfiguration Set(Func<GraphPosition, GraphPath, GraphPath> createPath)
        //{
        //    Contract.Requires(createPath != null);
        //    CreatePath = createPath;
        //    return this;
        //}

        //public IGraphTraversalConfiguration SetDirection(Direction direction)
        //{
        //    if (IncidencesIterator.Direction != direction)
        //        IncidencesIterator = new DefaultIncidencesIterator(direction);
        //    return this;
        //}

        #endregion Methods of TraversalQuery (6)
    }
}