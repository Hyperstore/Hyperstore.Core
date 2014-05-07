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
 
namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for identifier generator.
    /// </summary>
    /// <seealso cref="T:IDomainService"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IIdGenerator : IDomainService
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current value.
        /// </summary>
        /// <value>
        ///  The current value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string CurrentValue { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Calculate a new value.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema element 
        /// </param>
        /// <returns>
        ///  A new identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        Identity NextValue(ISchemaElement schemaElement);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  This method is called whenever a new element is added to the domain. 
        /// </summary>
        /// <param name="id">
        ///  An identifier
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Set(Identity id);
    }
}