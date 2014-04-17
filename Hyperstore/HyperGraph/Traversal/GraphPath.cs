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
 
#region Imports

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

        internal GraphPath(GraphPosition pos, GraphPath basePath = null)
        {
            DebugContract.Requires(pos, "pos");

            Elements = new List<IModelElement>();
            Relationships = new List<IModelRelationship>();

            if (basePath != null)
            {
                Elements.AddRange(basePath.Elements);
                Relationships.AddRange(basePath.Relationships);
            }

            Push(pos);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <value>
        ///  The relationships.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public List<IModelRelationship> Relationships { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the elements.
        /// </summary>
        /// <value>
        ///  The elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public List<IModelElement> Elements { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start element.
        /// </summary>
        /// <value>
        ///  The start element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement StartElement
        {
            get { return Elements.First(); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end element.
        /// </summary>
        /// <value>
        ///  The end element.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement EndElement
        {
            get { return Elements.Last(); }
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
            get { return Relationships.Count; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the last traversed relationship.
        /// </summary>
        /// <value>
        ///  The last traversed relationship.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship LastTraversedRelationship
        {
            get { return Relationships.Count > 0 ? Relationships.Last() : null; }
        }

        internal void Push(GraphPosition pos)
        {
            DebugContract.Requires(pos, "pos");

            Elements.Add(pos.Node);
            if (pos.FromEdge != null)
                Relationships.Add(pos.FromEdge);
            _hash = 0;
        }

        internal void Pop()
        {
            Elements.Remove(EndElement);
            if (Length > 0)
                Relationships.Remove(Relationships.Last());
            _hash = 0;
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
            var sb = new StringBuilder();
            if (Length > 0)
            {
                for (var i = 0; i < Elements.Count - 1; i++)
                {
                    sb.Append(Elements[i].Id);
                    sb.Append(" -- [");
                    sb.Append(Relationships[i].SchemaInfo.Id);
                    sb.Append("] --> ");
                }
                sb.Append(EndElement.Id);
            }
            return sb.ToString();
        }
    }
}