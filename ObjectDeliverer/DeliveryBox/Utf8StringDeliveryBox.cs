using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDeliverer.DeliveryBox
{
    public class Utf8StringDeliveryBox : DeliveryBoxBase
    {
        public delegate void CNUtf8StringDeliveryBoxReceived(string receivedString, ObjectDelivererProtocol fromObject);
        public event CNUtf8StringDeliveryBoxReceived? Received;

        public async ValueTask Send(string message)
        {
            await SendTo(message, null);
        }

        public ValueTask SendTo(string message, ObjectDelivererProtocol? destination)
        {
            if (objectDeliverer == null) return new ValueTask();

            return objectDeliverer.SendToAsync(UTF8Encoding.UTF8.GetBytes(message), destination!);
        }

        public override void NotifyReceiveBuffer(ObjectDelivererProtocol fromObject, Memory<byte> receivedBuffer)
        {
            Received?.Invoke(UTF8Encoding.UTF8.GetString(receivedBuffer.Span), fromObject);
        }
    }
}