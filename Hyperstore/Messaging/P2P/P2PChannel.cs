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
using System.ServiceModel;

#endregion

//http://www.codeproject.com/Articles/614028/Peer-to-Peer-File-Sharing-Through-WCF

namespace Hyperstore.Modeling.Messaging
{
#if !NETFX_CORE
    using System.ServiceModel.Channels;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A 2 p channel.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Messaging.WCFChannel"/>
    ///-------------------------------------------------------------------------------------------------
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PeerToPeerPChannel : WCFChannel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="address">
        ///  The address.
        /// </param>
        /// <param name="binding">
        ///  (Optional) the binding.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
#pragma warning disable 0618        
        public PeerToPeerPChannel(Uri address, Binding binding = null)
            : base(address, binding ?? new NetPeerTcpBinding
                                            {
                                                Security =
                                                {
                                                    Mode = SecurityMode.None
                                                }
                                            })
        {
        }
#pragma warning restore 0618
    }

#endif
}