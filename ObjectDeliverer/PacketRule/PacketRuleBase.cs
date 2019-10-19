using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
	public abstract class PacketRuleBase : IObservable<ReadOnlyMemory<byte>>
    {
        public enum ECNBufferEndian
        {
            /** Big Endian */
            Big = 0,
	        /** Little Endian */
	        Little
        };

		public abstract int WantSize { get; }

		public abstract PacketRuleBase Clone();

        protected ObjectDelivererProtocol? delivererProtocol;

        public void Initialize(ObjectDelivererProtocol delivererProtocol)
		{
            this.delivererProtocol = delivererProtocol;

            OnInitialize();
        }

        public virtual void OnInitialize()
        {
        }

        public abstract ValueTask MakeSendPacket(Memory<byte> bodyBuffer);

        public abstract void NotifyReceiveData(Memory<byte> dataBuffer);

		protected async ValueTask DispatchMadeSendBuffer(Memory<byte> memoryBuffer)
		{
            if (delivererProtocol == null) return;

            await delivererProtocol.RequestSendAsync(memoryBuffer);
		}

		protected void DispatchMadeReceiveBuffer(Memory<byte> receiveBuffer)
		{
            delivererProtocol?.RequestReceiveData(receiveBuffer);
		}

        public IDisposable Subscribe(IObserver<ReadOnlyMemory<byte>> observer)
        {
            throw new NotImplementedException();
        }
    }
}
