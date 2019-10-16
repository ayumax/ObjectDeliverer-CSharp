using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ObjectDeliverer.DeliveryBox
{
	public class ObjectDeliveryBoxUsingJson<T> : DeliveryBoxBase
	{
		public delegate void CNObjectDeliveryBoxReceived(T receivedObject, ObjectDelivererProtocol fromObject);
		public event CNObjectDeliveryBoxReceived? Received;


		public ObjectDeliveryBoxUsingJson()
		{

		}

		public override void NotifyReceiveBuffer(ObjectDelivererProtocol fromObject, Memory<byte> dataBuffer)
		{
			var createdObj = JsonSerializer.Deserialize<T>(dataBuffer.Span);

			if (createdObj != null)
			{
				Received?.Invoke(createdObj, fromObject);
			}	
		}

		public async ValueTask Send(T messageObject)
		{
            await SendTo(messageObject, null);
		}

		public async ValueTask SendTo(T messageObject, ObjectDelivererProtocol? destination)
		{
            if (objectDeliverer == null)
            {
                return;
            }

            var buffer = JsonSerializer.SerializeToUtf8Bytes<T>(messageObject);
            await objectDeliverer.SendTo(buffer, destination!);
		}
	}
}

