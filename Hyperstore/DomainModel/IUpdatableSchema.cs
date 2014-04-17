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
 
namespace Hyperstore.Modeling.Domain
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for updatable schema.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IUpdatableSchema
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the entity schema.
        /// </summary>
        /// <param name="id">
        ///  The id.
        /// </param>
        /// <param name="entitySchema">
        ///  The entity schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddEntitySchema(Identity id, ISchemaEntity entitySchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the property schema.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="propertySchema">
        ///  The property schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddPropertySchema(Identity id, ISchemaEntity propertySchema);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds the relationship schema.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="relationshipSchema">
        ///  The relationship schema.
        /// </param>
        /// <param name="startSchema">
        ///  The start schema.
        /// </param>
        /// <param name="endSchema">
        ///  The end schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void AddRelationshipSchema(Identity id, ISchemaRelationship relationshipSchema, ISchemaElement startSchema, ISchemaElement endSchema);
    }
}