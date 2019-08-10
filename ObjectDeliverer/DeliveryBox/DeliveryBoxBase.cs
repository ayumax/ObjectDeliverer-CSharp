using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;

namespace ObjectDeliverer.DeliveryBox
{
	public abstract class DeliveryBoxBase
    {
        public delegate void DeliveryBoxRequestSend(ObjectDelivererProtocol? protocol, Span<byte> sendBuffer);
        public event DeliveryBoxRequestSend RequestSend;

        public DeliveryBoxBase()
        {

        }

        public abstract void NotifyReceiveBuffer(ObjectDelivererProtocol FromObject, Span<byte> dataBuffer);

		protected void DispatchRequestSend(ObjectDelivererProtocol? protocol, Span<byte> sendBuffer)
        {
            RequestSend?.Invoke(protocol, sendBuffer);
        }
    }
    
}