using System;
using System.Collections.Generic;
using ObjectDeliverer.Protocol;
using ObjectDeliverer.DeliveryBox;
using ObjectDeliverer.PacketRule;

namespace ObjectDeliverer
{
	public class ObjectDelivererManager : IDisposable
	{
		public delegate void ObjectDelivererManagerConnected(ObjectDelivererProtocol delivererProtocol);
        public event ObjectDelivererManagerConnected Connected;
        public delegate void ObjectDelivererManagerDisconnected(ObjectDelivererProtocol delivererProtocol);
        public event ObjectDelivererManagerDisconnected Disconnected;
        public delegate void ObjectDelivererManagerReceiveData(ObjectDelivererProtocol delivererProtocol, Span<byte> dataBuffer);
        public event ObjectDelivererManagerReceiveData Received;

        private ObjectDelivererProtocol? CurrentProtocol;
		private DeliveryBoxBase? DeliveryBox;

		static ObjectDelivererManager CreateObjectDelivererManager(bool IsEventWithGameThread = true) => new ObjectDelivererManager();

		public ObjectDelivererManager()
		{
		}

		public void Start(ObjectDelivererProtocol Protocol, PacketRuleBase PacketRule, DeliveryBoxBase? DeliveryBox = null)
		{
			if (Protocol == null || PacketRule == null) return;

			CurrentProtocol = Protocol;
			CurrentProtocol.SetPacketRule(PacketRule);

			this.DeliveryBox = DeliveryBox;
			if (DeliveryBox != null)
			{
                DeliveryBox.RequestSend += DeliveryBox_RequestSend;
            }

            CurrentProtocol.Connected += CurrentProtocol_Connected;
            CurrentProtocol.Disconnected += CurrentProtocol_Disconnected;
            CurrentProtocol.ReceiveData += CurrentProtocol_ReceiveData;

            ConnectedList.Clear();

            CurrentProtocol.Start();
        }

        private void DeliveryBox_RequestSend(ObjectDelivererProtocol? destination, Span<byte> sendBuffer)
        {
            if (destination != null)
            {
                SendTo(sendBuffer, destination);
            }
            else
            {
                Send(sendBuffer);
            }
        }

        private void CurrentProtocol_ReceiveData(ObjectDelivererProtocol delivererProtocol, Span<byte> receivedBuffer)
        {
            Received?.Invoke(delivererProtocol, receivedBuffer);
            DeliveryBox?.NotifyReceiveBuffer(delivererProtocol, receivedBuffer);
        }

        private void CurrentProtocol_Disconnected(ObjectDelivererProtocol delivererProtocol)
        {
            ConnectedList.Remove(delivererProtocol);
            Disconnected?.Invoke(delivererProtocol);
        }

        private void CurrentProtocol_Connected(ObjectDelivererProtocol delivererProtocol)
        {
            ConnectedList.Add(delivererProtocol);
            Connected?.Invoke(delivererProtocol);
        }

        public void Close()
		{
			if (CurrentProtocol == null) return;

            if (DeliveryBox != null)
            {
                DeliveryBox.RequestSend -= DeliveryBox_RequestSend;
            }

            CurrentProtocol.Connected -= CurrentProtocol_Connected;
            CurrentProtocol.Disconnected -= CurrentProtocol_Disconnected;
            CurrentProtocol.ReceiveData -= CurrentProtocol_ReceiveData;

			CurrentProtocol.Close();

			CurrentProtocol = null;
		}

		public void Send(Span<byte> DataBuffer)
		{
			if (CurrentProtocol == null) return;
			if (disposedValue) return;

			CurrentProtocol.Send(DataBuffer);
		}

		public void SendTo(Span<byte> DataBuffer, ObjectDelivererProtocol Target)
		{
			if (CurrentProtocol == null) return;
			if (disposedValue) return;

			Target.Send(DataBuffer);
		}


		bool IsConnected => ConnectedList.Count > 0;

        private List<ObjectDelivererProtocol> ConnectedList = new List<ObjectDelivererProtocol>();

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Close();
				}

				disposedValue = true;
			}
		}


		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
