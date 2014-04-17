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
using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.DomainExtension
{
    internal interface IDomainInfos<T> where T : class,IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Activates the given domain.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Activate(T domain);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets domain model.
        /// </summary>
        /// <param name="sessionId">
        ///  Identifier for the session.
        /// </param>
        /// <returns>
        ///  The domain model.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        T GetDomainModel(Guid? sessionId);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Unloads.
        /// </summary>
        /// <param name="activeSessions">
        ///  The active sessions.
        /// </param>
        /// <param name="extension">
        ///  The extension.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool Unload(List<Guid> activeSessions, T extension);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the session completed action.
        /// </summary>
        /// <param name="guid">
        ///  Unique identifier.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool OnSessionCompleted(Guid guid);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Query if 'name' is extension name exists.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  true if extension name exists, false if not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IsExtensionNameExists(string name);
    }
}