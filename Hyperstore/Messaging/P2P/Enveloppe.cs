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
using System.Linq;
using System;
using System.Runtime.Serialization;
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Platform;
using System.Collections.Generic;
using System.Reflection;
#endregion

namespace Hyperstore.Modeling.Messaging
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An enveloppe.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [DataContract]
    public class Enveloppe
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="event">
        ///  The event.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public Enveloppe(IEvent @event)
        {
            Contract.Requires(@event, "@event");

            var dic = new Dictionary<string, object>();

            data = PlatformServices.Current.ObjectSerializer.Serialize(@event);
            eventName = @event.GetType().Name;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public Enveloppe()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the type of the event.
        /// </summary>
        /// <value>
        ///  The type of the event.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [DataMember]
        public string eventName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the body.
        /// </summary>
        /// <value>
        ///  The body.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [DataMember]
        public string data { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Deserialize event.
        /// </summary>
        /// <returns>
        ///  An IEvent.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEvent DeserializeEvent()
        {
            var name = eventName;
            var eventType = Type.GetType(name, false);
            if (eventType == null)
            {
                name = "Hyperstore.Modeling.Events." + name;
                eventType = Type.GetType(name, false);
                if (eventType == null)
                    return null;
            }

            var ctor = eventType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (ctor == null)
                return null;

            return (IEvent) PlatformServices.Current.ObjectSerializer.Deserialize(data, null, ctor.Invoke(null));
        }

    }
}