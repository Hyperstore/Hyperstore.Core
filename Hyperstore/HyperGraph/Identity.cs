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

using System;
using System.Diagnostics;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An identity.
    /// </summary>
    /// <seealso cref="T:System.IComparable{Hyperstore.Modeling.Identity}"/>
    /// <seealso cref="T:System.IEquatable{Hyperstore.Modeling.Identity}"/>
    /// <seealso cref="T:System.IComparable"/>
    ///-------------------------------------------------------------------------------------------------
    [DebuggerDisplay("{_value}")]
    public class Identity : IComparable<Identity>, IEquatable<Identity>, IComparable
    {
        private const char Separator = ':';
        private const string EmptyId = "NULL"; // Pas de risque de doublon car les clés sont tjs mises en minuscules

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The empty.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public static readonly Identity Empty = new Identity
            {
                _key = null,
                _domainModelName = null,
                _value = EmptyId,
                _hash = 0
            };
        private string _domainModelName;

        private int _hash;
        private string _key;
        private string _value;

        private Identity()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModelName">
        ///  The name of the domain model.
        /// </param>
        /// <param name="key">
        ///  The key.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public Identity(string domainModelName, string key)
        {
            Contract.RequiresNotEmpty(domainModelName, "domainModelName");
            Contract.RequiresNotEmpty(key, "key");
            Contract.Requires(key.IndexOf(Separator) < 0, "key cannot contains " + Separator);

            _key = key;
            _domainModelName = domainModelName;
            _value = (_domainModelName + Separator + _key).ToLowerInvariant();
            _hash = _value.GetHashCode();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the key.
        /// </summary>
        /// <value>
        ///  The key.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string Key
        {
            [DebuggerStepThrough]
            get { return _key; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the name of the domain model.
        /// </summary>
        /// <value>
        ///  The name of the domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string DomainModelName
        {
            [DebuggerStepThrough]
            get { return _domainModelName; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///  true if this instance is empty, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool IsEmpty
        {
            [DebuggerStepThrough]
            get { return _key == null; }
        }

        #region IComparable<Identity> Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Compares this Identity object to another to determine their relative ordering.
        /// </summary>
        /// <param name="other">
        ///  Another instance to compare.
        /// </param>
        /// <returns>
        ///  Negative if this instance is less than the other, 0 if they are equal, or positive if this is
        ///  greater.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int CompareTo(Identity other)
        {
            if (other == null)
                return -1;
            return String.Compare(_value, other._value, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region IEquatable<Identity> Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Tests if this Identity is considered equal to another.
        /// </summary>
        /// <param name="other">
        ///  The identity to compare to this instance.
        /// </param>
        /// <returns>
        ///  true if the objects are considered equal, false if they are not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool Equals(Identity other)
        {
            return other != null && _hash == other._hash && String.Compare(_value, other._value, StringComparison.OrdinalIgnoreCase) == 0;
        }

        #endregion

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Compares the current instance with another object of the same type and returns an integer
        ///  that indicates whether the current instance precedes, follows, or occurs in the same position
        ///  in the sort order as the other object.
        /// </summary>
        /// <param name="obj">
        ///  The object to compare with the current object.
        /// </param>
        /// <returns>
        ///  A value that indicates the relative order of the objects being compared. The return value has
        ///  these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" />
        ///  in the sort order. Zero This instance occurs in the same position in the sort order as
        ///  <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in
        ///  the sort order.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public int CompareTo(object obj)
        {
            if (obj is Identity)
                return CompareTo((Identity)obj);
            return 0;
        }

        //      [DebuggerStepThrough]

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Parses.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///  Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="str">
        ///  The.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static Identity Parse(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Invalid identity");

            var pos = str.LastIndexOf(Separator);
            if (pos < 0)
                throw new Exception(ExceptionMessages.InvalidIdentity);

            var dm = str.Substring(0, pos);
            var key = str.Substring(pos + 1);

            key = Conventions.ExtractMetaElementName(dm, key);

            return new Identity(dm, key);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Attempts to parse from the given data.
        /// </summary>
        /// <param name="str">
        ///  The.
        /// </param>
        /// <param name="id">
        ///  [out] The identifier.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public static bool TryParse(string str, out Identity id)
        {
            try
            {
                id = Parse(str);
                return true;
            }
            catch
            {
                id = Empty;
                return false;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public override string ToString()
        {
            return _value;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///  A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return _hash;
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
            if (obj is Identity)
                return Equals((Identity)obj);

            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Inequality operator.
        /// </summary>
        /// <param name="a">
        ///  The Identity to process.
        /// </param>
        /// <param name="b">
        ///  The Identity to process.
        /// </param>
        /// <returns>
        ///  The result of the operation.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static bool operator !=(Identity a, Identity b)
        {
            if (ReferenceEquals(a, b))
                return false;

            if (a == null)
                return b != null;

            return !a.Equals(b);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Equality operator.
        /// </summary>
        /// <param name="a">
        ///  The Identity to process.
        /// </param>
        /// <param name="b">
        ///  The Identity to process.
        /// </param>
        /// <returns>
        ///  The result of the operation.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static bool operator ==(Identity a, Identity b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if ((object)a == null || (object)b == null)
                return false;

            return a.Equals(b);
        }

        // Pas une bonne idée
        //public static implicit operator Identity(string identity)
        //{
        //    if (identity == null)
        //    {
        //        return Identity.Empty;
        //    }
        //    return Identity.Parse(identity);
        //}

        //[System.Web.Script.Serialization.ScriptIgnore]
        //Identity IHandle.Id
        //{
        //    [DebuggerStepThrough]
        //    get { return this; }
        //}

        // [System.Web.Script.Serialization.ScriptIgnore]

        internal Identity CreateAttributeIdentity(string propertyName)
        {
            return new Identity(DomainModelName, Key + propertyName);
        }

        internal Identity CreateMetaPropertyIdentity(string propertyName)
        {
            return new Identity(DomainModelName, Key + "P_" + propertyName);
        }
    }
}