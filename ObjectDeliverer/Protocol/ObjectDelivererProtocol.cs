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
        public delegate void ObjectDelivererProtocolReceiveData(ObjectDelivererProtocol delivererProtocol, Memory<byte> receivedBuffer);
        public event ObjectDelivererProtocolReceiveData? ReceiveData;

        protected PacketRuleBase PacketRule = PacketRuleFactory.CreatePacketRuleNodivision();

        public ObjectDelivererProtocol()
        {
        }

        public abstract ValueTask Start();
        public abstract ValueTask Close();

        public abstract ValueTask Send(Memory<byte> dataBuffer);

        protected virtual void DispatchConnected(ObjectDelivererProtocol delivererProtocol)
        {
            Connected?.Invoke(delivererProtocol);
        }

        protected virtual void DispatchDisconnected(ObjectDelivererProtocol delivererProtocol)
        {
            Disconnected?.Invoke(delivererProtocol);
        }

        protected virtual void DispatchReceiveData(ObjectDelivererProtocol delivererProtocol, Memory<byte> receivedBuffer)
        {
            ReceiveData?.Invoke(delivererProtocol, receivedBuffer);
        }

        public void SetPacketRule(PacketRuleBase PacketRule)
        {
            this.PacketRule = PacketRule;
            PacketRule.Initialize(this);
        }

        public abstract ValueTask RequestSend(Memory<byte> dataBuffer);
        public void RequestReceiveData(Memory<byte> dataBuffer)
        {
            DispatchReceiveData(this, dataBuffer);
        }


        #region IDisposable Support
        private bool disposedValue = false; // èdï°Ç∑ÇÈåƒÇ—èoÇµÇåüèoÇ∑ÇÈÇ…ÇÕ

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