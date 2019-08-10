using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace ObjectDeliverer.DeliveryBox
{
    public class Utf8StringDeliveryBox : DeliveryBoxBase
    {
        public delegate void CNUtf8StringDeliveryBoxReceived(string receivedString, ObjectDelivererProtocol fromObject);
        public event CNUtf8StringDeliveryBoxReceived Received;

        public void Send(string message)
        {
            SendTo(message, null);
        }

        public void SendTo(string message, ObjectDelivererProtocol? destination)
        {
            DispatchRequestSend(destination, UTF8Encoding.UTF8.GetBytes(message));
        }

        public override void NotifyReceiveBuffer(ObjectDelivererProtocol fromObject, Span<byte> receivedBuffer)
        {
            Received?.Invoke(UTF8Encoding.UTF8.GetString(receivedBuffer), fromObject);
        }
    }
}