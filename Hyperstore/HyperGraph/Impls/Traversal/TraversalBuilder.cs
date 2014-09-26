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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.HyperGraph
{
    public sealed class TraversalBuilder
    {
        private ITraversalQuery _query;

        internal TraversalBuilder()
        {
            _query = new TraversalQuery();
        }

        public TraversalBuilder UnicityPolicy(IGraphTraversalUnicityPolicy policy)
        {
            Contract.Requires(policy, "policy");
            _query.UnicityPolicy = policy;
            return this;
        }

        public TraversalBuilder NodeIterator(INodeIncidenceIterator iterator)
        {
            Contract.Requires(iterator, "iterator");
            _query.IncidencesIterator = iterator;
            return this;
        }

        public TraversalBuilder PathTraverser(IGraphPathTraverser traverser)
        {
            Contract.Requires(traverser, "traverser");
            _query.PathTraverser = traverser;
            return this;
        }
    }
}
