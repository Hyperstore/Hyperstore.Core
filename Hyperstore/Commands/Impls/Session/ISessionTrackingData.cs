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
        ///  Gets the involved tracked elements.
        /// </summary>
        /// <value>
        ///  The involved elements.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<TrackedElement> InvolvedTrackedElements { get; }

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
        IEnumerable<TrackedElement> GetTrackedElementsByState(TrackingState state);

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
        TrackingState GetTrackedElementState(Identity id);
    }
}