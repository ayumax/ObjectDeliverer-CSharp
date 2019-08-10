using System;
using System.Collections.Generic;

namespace ObjectDeliverer.PacketRule
{
	public abstract class PacketRuleBase
	{
        public enum ECNBufferEndian
        {
            /** Big Endian */
            Big = 0,
	        /** Little Endian */
	        Little
        };


        public delegate void CNPacketRuleMadeSendBuffer(Span<byte> sendBuffer);
		public event CNPacketRuleMadeSendBuffer MadeSendBuffer;

		public delegate void CNPacketRuleMadeReceiveBuffer(byte[] receiveBuffer);
		public event CNPacketRuleMadeReceiveBuffer MadeReceiveBuffer;


		public abstract int WantSize { get; }

		public abstract PacketRuleBase Clone();

		public virtual void Initialize()
		{

		}

        public abstract void MakeSendPacket(byte[] bodyBuffer);

        public abstract void NotifyReceiveData(byte[] dataBuffer);

		protected void DispatchMadeSendBuffer(Span<byte> sendBuffer)
		{
			MadeSendBuffer?.Invoke(sendBuffer);
		}

		protected void DispatchMadeReceiveBuffer(byte[] receiveBuffer)
		{
			MadeReceiveBuffer?.Invoke(receiveBuffer);
		}
	}
}
