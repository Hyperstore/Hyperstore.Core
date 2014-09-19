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
using System.Runtime.Serialization;
using Hyperstore.Modeling.Events;
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Platform;

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
            if (@event is ICustomEventSerializer)
            {
                Data = ((ICustomEventSerializer)@event).Serialize();
                Flags |= 0x01;
            }
            else
            {
                Data = PlatformServices.Current.ObjectSerializer.Serialize(@event);
            }
            EventType = Hyperstore.Modeling.Utils.ReflectionHelper.GetNameWithSimpleAssemblyName(@event.GetType());
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
        public string EventType { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the body.
        /// </summary>
        /// <value>
        ///  The body.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [DataMember]
        public string Data { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the identifier of the message.
        /// </summary>
        /// <value>
        ///  The identifier of the message.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [DataMember]
        public string MessageId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the flags.
        /// </summary>
        /// <value>
        ///  The flags.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [DataMember]
        public byte Flags { get; set; }

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
            var eventType = Type.GetType(EventType, false);
            if (eventType == null)
                return null; // TODO

            if ((Flags & 0x01) == 0x01)
            {
                var evt = Activator.CreateInstance(eventType) as ICustomEventSerializer;
                if (evt != null)
                {
                    evt.Deserialize(Data);
                    return evt;
                }
            }

            return (IEvent) PlatformServices.Current.ObjectSerializer.Deserialize(eventType, Data, null);
        }
    }
}