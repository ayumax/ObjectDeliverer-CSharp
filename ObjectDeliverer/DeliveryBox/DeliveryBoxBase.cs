using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;

namespace ObjectDeliverer.DeliveryBox
{
	public abstract class DeliveryBoxBase
    {
        public ObjectDelivererManager? objectDeliverer { protected get; set; }

        public DeliveryBoxBase()
        {

        }

        public abstract void NotifyReceiveBuffer(ObjectDelivererProtocol FromObject, ReadOnlyMemory<byte> dataBuffer);
    }
    
}