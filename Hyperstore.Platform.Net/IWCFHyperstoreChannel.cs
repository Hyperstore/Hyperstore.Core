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

using System.ServiceModel;

#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for iwcf hyperstore channel.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [ServiceContract(CallbackContract = typeof (IWCFHyperstoreChannel))]
    public interface IWCFHyperstoreChannel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Process the events described by data.
        /// </summary>
        /// <param name="data">
        ///  The data.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        [OperationContract(IsOneWay = true)]
        void ProcessEvents(Message data);
    }
}