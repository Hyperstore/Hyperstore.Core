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

using System;
using System.ServiceModel;

#endregion

//http://www.codeproject.com/Articles/614028/Peer-to-Peer-File-Sharing-Through-WCF

namespace Hyperstore.Modeling.Messaging
{
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
}