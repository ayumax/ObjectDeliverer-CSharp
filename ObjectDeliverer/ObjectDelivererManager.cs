using System;
using System.Collections.Generic;
using ObjectDeliverer.Protocol;
using ObjectDeliverer.DeliveryBox;
using ObjectDeliverer.PacketRule;
using System.Threading.Tasks;
using System.Reactive;

namespace ObjectDeliverer
{
	public class ObjectDelivererManager : IDisposable
	{
        public delegate void ObjectDelivererManagerConnected(IProtocol delivererProtocol);
        public event ObjectDelivererManagerDisconnected? Connected;
        public delegate void ObjectDelivererManagerDisconnected(IProtocol delivererProtocol);
        public event ObjectDelivererManagerDisconnected? Disconnected;
        public delegate void ObjectDelivererManagerReceiveData(IProtocol delivererProtocol, Memory<byte> dataBuffer);
        public event ObjectDelivererManagerReceiveData? Received;

        private ObjectDelivererProtocol? CurrentProtocol;
		private DeliveryBoxBase? DeliveryBox;

		public static ObjectDelivererManager CreateObjectDelivererManager(bool IsEventWithGameThread = true) => new ObjectDelivererManager();

		public ObjectDelivererManager()
		{
        }

		public ValueTask StartAsync(ObjectDelivererProtocol Protocol, PacketRuleBase PacketRule, DeliveryBoxBase? DeliveryBox = null)
		{
            if (Protocol == null || PacketRule == null) return new ValueTask();

			CurrentProtocol = Protocol;
			CurrentProtocol.SetPacketRule(PacketRule);

			this.DeliveryBox = DeliveryBox;
			if (DeliveryBox != null)
			{
                DeliveryBox.objectDeliverer = this;
            }

            CurrentProtocol.Connected += CurrentProtocol_Connected;
            CurrentProtocol.Disconnected += CurrentProtocol_Disconnected;
            CurrentProtocol.ReceiveData += CurrentProtocol_ReceiveData;

            ConnectedList.Clear();

            return CurrentProtocol.StartAsync();
        }


        private void CurrentProtocol_ReceiveData(ObjectDelivererProtocol delivererProtocol, Memory<byte> receivedBuffer)
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

        public async ValueTask CloseAsync()
		{
			if (CurrentProtocol == null) return;

            CurrentProtocol.Connected -= CurrentProtocol_Connected;
            CurrentProtocol.Disconnected -= CurrentProtocol_Disconnected;
            CurrentProtocol.ReceiveData -= CurrentProtocol_ReceiveData;

			await CurrentProtocol.CloseAsync();

			CurrentProtocol = null;
		}

		public ValueTask SendAsync(Memory<byte> DataBuffer)
		{
            if (CurrentProtocol == null || disposedValue) return new ValueTask();

            return CurrentProtocol.SendAsync(DataBuffer);
		}

		public ValueTask SendToAsync(Memory<byte> DataBuffer, IProtocol Target)
		{
            if (CurrentProtocol == null || disposedValue) return new ValueTask();

            if (Target is ObjectDelivererProtocol delivererProtocol)
            {
                return delivererProtocol.SendAsync(DataBuffer);
            }

            return new ValueTask();
		}


		bool IsConnected => ConnectedList.Count > 0;

        private List<ObjectDelivererProtocol> ConnectedList = new List<ObjectDelivererProtocol>();

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual async void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					await CloseAsync();
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
