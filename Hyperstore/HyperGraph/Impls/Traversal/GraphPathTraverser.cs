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
using Hyperstore.Modeling.Container;
using Hyperstore.Modeling.HyperGraph;
using System;

#endregion

namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A graph path traverser.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Traversal.IGraphPathTraverser"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class GraphPathTraverser : IGraphPathTraverser
    {
        private ITraversalQuery _query;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes this instance.
        /// </summary>
        /// <param name="query">
        ///  The query.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void IGraphPathTraverser.Initialize(ITraversalQuery query)
        {
            Contract.Requires(query, "query");
            _query = query;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates the items in this collection that meet given criteria.
        /// </summary>
        /// <param name="node">
        ///  The node.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the matched items.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<GraphPath> Traverse(NodeInfo node)
        {
            DebugContract.Requires(node);

            // Création du container de stockage des noeuds (ou plutot des chemins jusqu'à ce noeud) restant à traverser
            // Le type est tributaire de l'algorithme de traversé (Queue pour BreadthFirst et Stack pour DepthFirst)
            // Ce container est mis à jour à chaque noeud.
            var paths = CreatePathContainer();

            // Lecture du noeud de départ            
            if (node == null)
                yield break;

            // Constitution du Path courant
            var path = new GraphPath(_query.DomainModel, node);

            // Initialisation du container avec le 1er noeud
            paths.Insert(new[] { path });

            // Tant qu'il y a des noeuds à parcourir
            while (!paths.IsEmpty)
            {
                // On prend le prochain noeud à parcourir (dépend de l'algo de traversé)
                path = paths.Retrieve();

                // On indique que ce noeud a été visité
                _query.UnicityPolicy.MarkVisited(path);

                // Filtrage du chemin courant pour savoir si on continue
                var result = _query.Evaluator.Visit(path);

                // Le chemin courant est à prendre en compte
                if ((GraphTraversalEvaluatorResult.Include & result) == GraphTraversalEvaluatorResult.Include)
                {
//                    _trace.WriteTrace(TraceCategory.Traverser, "Include : {0}", path);
                    yield return path;
                }

                // Arrêt forcé
                if ((result & GraphTraversalEvaluatorResult.Exit) == GraphTraversalEvaluatorResult.Exit)
                    break;

                if ((result & GraphTraversalEvaluatorResult.Continue) == GraphTraversalEvaluatorResult.Continue)
                {
                    var childPaths = new List<GraphPath>(31);

                    // Parcours de toutes les relations du noeud courant pour tester si
                    // on les prend en compte
                    foreach (var rel in _query.IncidencesIterator.From(path.EndElement))
                    {
  //                      _trace.WriteTrace(TraceCategory.Traverser, "Visit : {1} for {0}", path, rel.Id);

                        NodeInfo childNode = null;
                        if (String.Compare(rel.EndId.DomainModelName, _query.DomainModel.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            childNode = new NodeInfo(rel.EndId, rel.EndSchemaId);
                        
                        var p = path.Create(childNode, rel);

                        // Si ce chemin n'a pas dèjà été traité, on l'ajoute dans la liste des chemins à traiter
                        if (!_query.UnicityPolicy.IsVisited(p))
                        {
  //                          _trace.WriteTrace(TraceCategory.Traverser, "Push : {0}", p);
                            childPaths.Add(p);
                        }
                    }
                    paths.Insert(childPaths);
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates path container.
        /// </summary>
        /// <returns>
        ///  The new path container.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected abstract IGraphPathList CreatePathContainer();

    }
}