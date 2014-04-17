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
 
namespace Hyperstore.Modeling.Validations
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for constraint.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IConstraint
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the category.
        /// </summary>
        /// <value>
        ///  The category.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        string Category { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Apply on the specified value.
        /// </summary>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <param name="log">
        ///  The log.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void Apply(IModelElement value, ISessionContext log);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Check if the constraint can be applied on element of the specific schema element.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <returns>
        ///  true if it succeeds, false if it fails.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool ApplyOn(ISession session, ISchemaElement schemaElement);
    }
}