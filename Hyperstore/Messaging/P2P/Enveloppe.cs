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
                Body = ((ICustomEventSerializer)@event).Serialize();
                Flags |= 0x01;
            }
            else
            {
                Body = PlatformServices.Current.ObjectSerializer.Serialize(@event);
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
        public string Body { get; set; }

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
        ///  Gets or sets the identifier of the correlation.
        /// </summary>
        /// <value>
        ///  The identifier of the correlation.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        [DataMember]
        public Guid CorrelationId { get; set; }

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
                    evt.Deserialize(Body);
                    return evt;
                }
            }

            return (IEvent) PlatformServices.Current.ObjectSerializer.Deserialize(eventType, Body, null);
        }
    }
}