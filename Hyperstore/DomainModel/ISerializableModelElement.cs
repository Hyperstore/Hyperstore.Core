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
 
namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for serializable model element.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface ISerializableModelElement
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [deserializing].
        /// </summary>
        /// <param name="containerSchema">
        ///  The container schema.
        /// </param>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="start">
        ///  (Optional) The start.
        /// </param>
        /// <param name="end">
        ///  (Optional) The end.
        /// </param>
        /// <param name="endSchemaId">
        ///  (Optional) the end schema identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void OnDeserializing(ISchemaElement containerSchema, IDomainModel domainModel, string key, Identity start = null, Identity end = null, Identity endSchemaId=null);
    }
}