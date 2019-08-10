using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using Utf8Json;

namespace ObjectDeliverer.DeliveryBox
{
	public class ObjectDeliveryBoxUsingJson<T> : DeliveryBoxBase
	{
		public delegate void CNObjectDeliveryBoxReceived(T receivedObject, ObjectDelivererProtocol fromObject);
		public event CNObjectDeliveryBoxReceived Received;


		public ObjectDeliveryBoxUsingJson()
		{

		}

		public override void NotifyReceiveBuffer(ObjectDelivererProtocol fromObject, Span<byte> dataBuffer)
		{
			var createdObj = JsonSerializer.Deserialize<T>(dataBuffer.ToArray());

			if (createdObj != null)
			{
				Received?.Invoke(createdObj, fromObject);
			}	
		}

		public void Send(T messageObject)
		{
			SendTo(messageObject, null);
		}

		public void SendTo(T messageObject, ObjectDelivererProtocol? destination)
		{
			var buffer = JsonSerializer.Serialize(messageObject);

			DispatchRequestSend(destination, buffer);
		}
	}
}

