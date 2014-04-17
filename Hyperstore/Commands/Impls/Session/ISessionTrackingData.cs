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
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Tracks all involved elements during a session.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface ISessionTrackingData
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the involved tracking elements.
        /// </summary>
        /// <value>
        ///  The involved elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<TrackingElement> InvolvedTrackingElements { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the involved model elements.
        /// </summary>
        /// <remarks>
        ///  This method is only valid when the session is being disposed (Constraint or notifications)
        /// </remarks>
        /// <value>
        ///  The involved model elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelElement> InvolvedModelElements { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets elements by state.
        /// </summary>
        /// <param name="state">
        ///  The state.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the tracking elements by states in
        ///  this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<TrackingElement> GetTrackingElementsByState(TrackingState state);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the state of an element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <returns>
        ///  The tracking element state.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        TrackingState GetTrackingElementState(Identity id);
    }
}