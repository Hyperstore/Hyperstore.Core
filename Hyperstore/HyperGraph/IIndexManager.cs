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

using System;

#endregion

namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for index manager.
    /// </summary>
    /// <seealso cref="T:IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IIndexManager : IDisposable
    {
        //  void CreateIndex( IMetaClass metaclass, string name, bool unique, params IMetaProperty[] properties );

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the index.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema container.
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="unique">
        ///  if set to <c>true</c> [unique].
        /// </param>
        /// <param name="propertyNames">
        ///  The property names.
        /// </param>
        /// <returns>
        ///  The new index.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IIndex CreateIndex(ISchemaElement schemaElement, string name, bool unique, params string[] propertyNames);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Drops the index.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void DropIndex(string name);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Retourne un index.
        /// </summary>
        /// <remarks>
        ///  L'index doit être session safe (toutes les opérations doivent s'assurer d'être faites dans
        ///  une session)
        /// </remarks>
        /// <param name="name">
        ///  .
        /// </param>
        /// <returns>
        ///  The index.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IIndex GetIndex(string name);
    }
}