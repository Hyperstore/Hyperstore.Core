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

using Hyperstore.Modeling.HyperGraph;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Hyperstore.Modeling.Traversal
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A graph path.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class GraphPath
    {
        private int _hash;
        private readonly GraphPath _parent;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel DomainModel { get; private set; }


        internal GraphPath(IDomainModel domain, NodeInfo node)
        {
            DebugContract.Requires(node, "node");

            DomainModel = domain;
            EndElement = node;
        }

        private GraphPath(GraphPath parent, NodeInfo node, EdgeInfo fromEdge)
        {
            DomainModel = parent.DomainModel;
            _parent = parent;
            EndElement = node;
            LastTraversedRelationship = fromEdge;
        }

        internal GraphPath Create(NodeInfo node, EdgeInfo fromEdge)
        {
            DebugContract.Requires(node);

            return new GraphPath(this, node, fromEdge);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <value>
        ///  The relationships.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<EdgeInfo> Relationships { get { return IterateToRoot().Where(p => p.LastTraversedRelationship != null).Select(p => p.LastTraversedRelationship); } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <value>
        ///  The elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<NodeInfo> Elements { get { return IterateToRoot().Where(p => p.EndElement != null).Select(p => p.EndElement); } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start element.
        /// </summary>
        /// <value>
        ///  The start element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public NodeInfo StartElement
        {
            get
            {
                var p = IterateToRoot().Last();
                return p.EndElement;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Enumerates iterate to root in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process iterate to root in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<GraphPath> IterateToRoot()
        {
            var p = this;
            for (; ; )
            {
                yield return p;
                if (p._parent == null)
                    break;
                p = p._parent;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end element.
        /// </summary>
        /// <value>
        ///  The end element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public NodeInfo EndElement
        {
            get;
            private set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the length.
        /// </summary>
        /// <value>
        ///  The length.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Length
        {
            get { return Relationships.Count(); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the last traversed relationship.
        /// </summary>
        /// <value>
        ///  The last traversed relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public EdgeInfo LastTraversedRelationship
        {
            get;
            private set;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///  <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">
        ///  The object to compare with the current object.
        /// </param>
        /// <returns>
        ///  true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            var path = obj as GraphPath;
            if (obj != null)
            {
                if (ReferenceEquals(this, obj))
                    return true;

                return Length == path.Length && GetHashCode() == path.GetHashCode();
            }

            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///  A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            if (_hash == 0)
            {
                _hash = 5381;
                foreach (var mel in Relationships)
                {
                    _hash = ((_hash << 5) + _hash) + mel.Id.GetHashCode();
                }
            }
            return _hash;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            var stack = new Stack<string>();

            var iterator = Relationships.GetEnumerator();
            foreach (var mel in Elements)
            {
                stack.Push("]");
                stack.Push(mel.Id.ToString());
                if (iterator.MoveNext())
                {
                    stack.Push(" --> [");
                    stack.Push(iterator.Current.SchemaId.ToString());
                    stack.Push(" -- ");
                }
            }
            stack.Push("[");

            return System.String.Concat(stack);
        }
    }
}