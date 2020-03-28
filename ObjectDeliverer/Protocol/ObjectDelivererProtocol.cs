using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ObjectDeliverer.PacketRule;

namespace ObjectDeliverer.Protocol
{
    public abstract class ObjectDelivererProtocol : IDisposable, IProtocol
    {
        public delegate void ObjectDelivererProtocolConnected(ObjectDelivererProtocol delivererProtocol);
        public event ObjectDelivererProtocolConnected? Connected;
        public delegate void ObjectDelivererProtocolDisconnected(ObjectDelivererProtocol delivererProtocol);
        public event ObjectDelivererProtocolDisconnected? Disconnected;
        public delegate void ObjectDelivererProtocolReceiveData(ObjectDelivererProtocol delivererProtocol, ReadOnlyMemory<byte> receivedBuffer);
        public event ObjectDelivererProtocolReceiveData? ReceiveData;

        protected PacketRuleBase PacketRule = PacketRuleFactory.CreatePacketRuleNodivision();

        public ObjectDelivererProtocol()
        {
        }

        public abstract ValueTask StartAsync();
        public abstract ValueTask CloseAsync();

        public abstract ValueTask SendAsync(Memory<byte> dataBuffer);

        protected virtual void DispatchConnected(ObjectDelivererProtocol delivererProtocol)
        {
            Connected?.Invoke(delivererProtocol);
        }

        protected virtual void DispatchDisconnected(ObjectDelivererProtocol delivererProtocol)
        {
            Disconnected?.Invoke(delivererProtocol);
        }

        protected virtual void DispatchReceiveData(ObjectDelivererProtocol delivererProtocol, ReadOnlyMemory<byte> receivedBuffer)
        {
            ReceiveData?.Invoke(delivererProtocol, receivedBuffer);
        }

        public void SetPacketRule(PacketRuleBase PacketRule)
        {
            this.PacketRule = PacketRule;
            PacketRule.Initialize();
        }


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