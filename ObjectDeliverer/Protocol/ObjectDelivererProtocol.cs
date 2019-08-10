using System;
using System.Collections.Generic;
using ObjectDeliverer.PacketRule;

namespace ObjectDeliverer.Protocol
{
    public abstract class ObjectDelivererProtocol : IDisposable
    {
        public delegate void ObjectDelivererProtocolConnected(ObjectDelivererProtocol delivererProtocol);
        public event ObjectDelivererProtocolConnected Connected;
        public delegate void ObjectDelivererProtocolDisconnected(ObjectDelivererProtocol delivererProtocol);
        public event ObjectDelivererProtocolDisconnected Disconnected;
        public delegate void ObjectDelivererProtocolReceiveData(ObjectDelivererProtocol delivererProtocol, byte[] receivedBuffer);
        public event ObjectDelivererProtocolReceiveData ReceiveData;

        protected PacketRuleBase? PacketRule;

        public ObjectDelivererProtocol()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Close()
        {
        }

        public virtual void Send(byte[] dataBuffer)
        {

        }

        protected void DispatchConnected(ObjectDelivererProtocol delivererProtocol)
        {
            Connected?.Invoke(delivererProtocol);
        }

        protected void DispatchDisconnected(ObjectDelivererProtocol delivererProtocol)
        {
            Disconnected?.Invoke(delivererProtocol);
        }

        protected void DispatchReceiveData(ObjectDelivererProtocol delivererProtocol, byte[] receivedBuffer)
        {
            ReceiveData?.Invoke(delivererProtocol, receivedBuffer);
        }

        public void SetPacketRule(PacketRuleBase PacketRule)
        {
            this.PacketRule = PacketRule;
            PacketRule.Initialize();

            PacketRule.MadeSendBuffer += x => RequestSend(x);
            PacketRule.MadeReceiveBuffer += x => DispatchReceiveData(this, x);
        }

        public abstract void RequestSend(byte[] dataBuffer);


        #region IDisposable Support
        private bool disposedValue = false; // d•¡‚·‚éŒÄ‚Ño‚µ‚ğŒŸo‚·‚é‚É‚Í

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