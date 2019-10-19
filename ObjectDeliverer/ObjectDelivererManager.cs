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

		public async ValueTask Start(ObjectDelivererProtocol Protocol, PacketRuleBase PacketRule, DeliveryBoxBase? DeliveryBox = null)
		{
			if (Protocol == null || PacketRule == null) return;

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

            await CurrentProtocol.StartAsync();
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

        public void Close()
		{
			if (CurrentProtocol == null) return;

            CurrentProtocol.Connected -= CurrentProtocol_Connected;
            CurrentProtocol.Disconnected -= CurrentProtocol_Disconnected;
            CurrentProtocol.ReceiveData -= CurrentProtocol_ReceiveData;

			CurrentProtocol.CloseAsync();

			CurrentProtocol = null;
		}

		public async ValueTask Send(Memory<byte> DataBuffer)
		{
			if (CurrentProtocol == null) return;
			if (disposedValue) return;

			await CurrentProtocol.SendAsync(DataBuffer);
		}

		public async ValueTask SendTo(Memory<byte> DataBuffer, ObjectDelivererProtocol Target)
		{
			if (CurrentProtocol == null) return;
			if (disposedValue) return;

			await Target.SendAsync(DataBuffer);
		}


		bool IsConnected => ConnectedList.Count > 0;

        private List<ObjectDelivererProtocol> ConnectedList = new List<ObjectDelivererProtocol>();

		#region IDisposable Support
		private bool disposedValue = false;

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
